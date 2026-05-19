using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

//Singleton rastreia qual jogador está usando qual skin.
//Verificação de conflito exclusiva do servidor. Pare evitar race condition.
//NetworkList propaga o estado para todos os clientes automaticamente.
public class SkinManager : NetworkBehaviour
{
    //Permite que qualquer classe leia SkinManager.Instace, mas só o SkinManager pode escrever.
    public static SkinManager Instance { get; private set; }

    [Header("Skins disponíveis (arraste os sprites aqui no Inspetor)")]
    [SerializeField] private Sprite[] skinSprites; //3 sprites dos sorvetes

    //Lista sincronizada pela rede.
    private NetworkList<SkinReservation> skinReservations = new();


    //Roda antes de todos ou outros Scripts. Inicialização interna do script.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    //----------------------------------------------------------------
    //Consultas locais para feedback visual na UI
    //----------------------------------------------------------------

    //Verifica se a skin está em uso.
    public bool IsSkinTaken(int skinIndex, ulong requestingClientId)
    {
        foreach (var reservation in skinReservations)
        {
            if (reservation.SkinIndex == skinIndex && reservation.ClientId != requestingClientId) //Tmabém permite reconfirmar skin já em uso sem negativa.
                return true;
        }
        return false;
    }

    //Retorna o sprite da skin. Valida o índice antes de acessar o array.
    public Sprite GetSkinSprite(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= skinSprites.Length) return null;
        return skinSprites[skinIndex];
    }

    //Get quantidade de skins
    public int GetTotalSkins() => skinSprites.Length;


    //----------------------------------------------------------------
    //Ponto de entrada
    //----------------------------------------------------------------
    // Cliente chama isso. Envia Rpc para o Session Owner validar.
    // Se já sou o Session Owner, processo direto
    public void RequestSkin(int newSkinIndex, ulong clientId)
    {
        ulong owner = NetworkManager.CurrentSessionOwner;

        if (NetworkManager.LocalClientId == owner)
            ProcessSkinRequest(newSkinIndex, clientId);
        else
            RequestSkinsRpc(newSkinIndex, clientId, RpcTarget.Single(owner, RpcTargetUse.Temp));

    }


    //----------------------------------------------------------------
    //Lógica autoritativa
    //----------------------------------------------------------------
    // Enviado pelo cliente, executado no Session Owner.
    [Rpc(SendTo.SpecifiedInParams)]
    private void RequestSkinsRpc(int newSkinIndex, ulong clientId, RpcParams rpcParams = default)
    {
        ProcessSkinRequest(newSkinIndex, clientId);
    }


    //Verificação oficial se o cliente pode mudar a skin.
    //Pedidos chegam um por vez para evitar condição de corrida.
    private void ProcessSkinRequest(int newSkinIndex, ulong clientId)
    {
        if (IsSkinTaken(newSkinIndex, clientId)) //skin em uso
        {
            SkinResultRpc(false, newSkinIndex, clientId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            return;
        }

        //Aprovado
        RemoveClientReservation(clientId);
        skinReservations.Add(new SkinReservation { SkinIndex = newSkinIndex, ClientId = clientId });
        SkinResultRpc(true, newSkinIndex, clientId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }


    //----------------------------------------------------------------
    //RPC DE RESPOSTA — executado no cliente que pediu
    //----------------------------------------------------------------

    //Função que percorre os clientes conectados até encontrar o buscado e retorna o resultado ao PlayerProfileData, pelo OnSkinRequestResult.
    [Rpc(SendTo.SpecifiedInParams)]
    private void SkinResultRpc(bool approved, int newSkinIndex, ulong targetClientId, RpcParams rpcParams = default)
    {
        foreach (var client in NetworkManager.ConnectedClients.Values)
        {
            if (client.ClientId == NetworkManager.LocalClientId)
            {
                var profile = client.PlayerObject?.GetComponent<PlayerProfileDados>();
                profile?.OnSkinRequestResult(approved, newSkinIndex);
                break;
            }
        }
    }


    //----------------------------------------------------------------
    //Liberação de reserva
    //----------------------------------------------------------------
    //Procura a entrada o cliente, remove e retorna, cada cliente só pode ter uma reserva.
    private void RemoveClientReservation(ulong clientId)
    {
        for (int i = 0; i < skinReservations.Count; i++)
        {
            if (skinReservations[i].ClientId == clientId)
            {
                skinReservations.RemoveAt(i);
                return;
            }
        }
    }


    //----------------------------------------------------------------
    //Liberação de desconexão
    //----------------------------------------------------------------
    //Inscreve no evento de desconexão do NetworkManager
    public override void OnNetworkSpawn()
    {
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }
    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.LocalClientId == NetworkManager.CurrentSessionOwner)
            RemoveClientReservation(clientId);
    }


    //----------------------------------------------------------------
    //Struct SkinReservation
    //----------------------------------------------------------------
    //Item da NetworkList
    //Struct ao invés de classe por ser mais simples.
    
    //Precisa ser serializados pois a troca de dados pela rede é por bytes.
    //Serializar é converter um objeto em bytes para enviar, desserializar é converter bytes de volta em objeto ao receber.
    //NetworkList exige INetworkSerializable (serialização) e IEquatble (para conseguir comparar entradas).

    public struct SkinReservation : INetworkSerializable, System.IEquatable<SkinReservation>
    {
        public int SkinIndex;
        public ulong ClientId;

        //Método de serialização, ensina o Netcode como converter esse struct para bytes e de volta.
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref SkinIndex); //ref permite que o serializador tanto leia quanto escreva o valor
            serializer.SerializeValue(ref ClientId);
        }

        //Método para conseguir compara entradas, importante para sincronização.
        public bool Equals(SkinReservation other) =>
            SkinIndex == other.SkinIndex && ClientId == other.ClientId;

    }
}