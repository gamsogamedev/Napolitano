using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


//Classe que guarda os dados do playerprofile e os sincroniza para todos.
//Cada cliente tem autoridade sobre o seu próprio objeto.
//A aprovação de skin passa pelo servidor pra evitar conflitos.


public class PlayerProfileDados : NetworkBehaviour
{
    //----------------------------------------------------------------
    //VARIÁVEIS
    //----------------------------------------------------------------
    //Variáveis que o Netcode sincroniza automaticamente pela rede.
    //Parâmetros: todos podem ler, mas apenas owner pode escrever.

    

    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<int> SkinIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    //Para travar essa skin ao jogador quando o jogo começar. Quando o start do Lobby é clicado.
    public NetworkVariable<bool> IsLocked = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    //Sinaliza que o jogador está pronto para iniciar. Owner escreve o próprio ready. O Lobby da sala lê isso para liberar start.
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);


    //Variável para saber o level máximo alcançado do host, utilizada pelo MapSelector
    public NetworkVariable<int> MaxLevel = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);


    //Propriedade de conveniência. Se o jogador tem uma skin valida confirmada o Lobby permite clicar em Ready.
    public bool ValidSkin => SkinIndex.Value >= 0;

    
    //Referência visual ao personagem
    [Header("Visual")]
    [SerializeField] private SpriteRenderer PersonagemRenderizado;

    //----------------------------------------------------------------
    //EVENTOS
    //----------------------------------------------------------------

    //Evento local chamado quando a skin muda _ UI e visual se inscrevem nele.
    public event System.Action OnProfileChanged;

    //Evento que dá feedback de aprovado ou negado da Skin.
    public event System.Action<bool, int> OnSkinConfirmationReceived;


    //----------------------------------------------------------------
    // CICLO DE VIDA DA REDE
    //----------------------------------------------------------------

    //Equivalente ao Start() para objetos de rede.
    //Spawn: Callback (passada como argumento para outra função) que disparam quando o valor das variáveis mudam.
    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnAnyValueChanged;
        SkinIndex.OnValueChanged += OnSkinChanged;
        IsReady.OnValueChanged += OnAnyValueChanged;
        
        if (IsOwner)
        {
            PlayerName.Value = new FixedString64Bytes($"Player_{OwnerClientId}");
        }
    }
    //Despawn desisncreve as callbacks quando o objeto morre, para não vazar memória.
    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnAnyValueChanged;
        SkinIndex.OnValueChanged -= OnSkinChanged;
        IsReady.OnValueChanged -= OnAnyValueChanged;
    }

    //Se há mudanças, esse método é chamado, para chamar o evento OnProfileChanged que atualiza automaticamente o UI e visual.
    private void OnAnyValueChanged<T>(T previus, T current)
    {
        OnProfileChanged?.Invoke();
    }
    private void OnSkinChanged(int previus, int current)
    {
        ApplyVisual(current);
        OnProfileChanged?.Invoke();
    }

    //----------------------------------------------------------------
    //
    //----------------------------------------------------------------

    //Envia o pedido de skin ao servidor. O resultado chega assíncrono via OnSkinRequestResult.
    public void RequestSkin(int newSkinIndex)
    {
        if (!IsOwner) return;
        if (IsLocked.Value) return;
        if (newSkinIndex == SkinIndex.Value) return;

        SkinManager.Instance.RequestSkin(newSkinIndex, OwnerClientId);
    }

    //Resposta do server para o pedido da skin, passado pelo SkinManager. Se aprovado, aplica a skin e propaga para todos via NetworkVariable.
    public void OnSkinRequestResult(bool approved, int newSkinIndex)
    {
        if (approved)
        {
            SkinIndex.Value = newSkinIndex;
            IsReady.Value = false; //Skin mudou, jogador perde o ready
        }

        OnSkinConfirmationReceived?.Invoke(approved, newSkinIndex); //Notifica UI do resultado.
    }

    public void ApplyName(string newName)
    {
        if (!IsOwner) return;
        if (string.IsNullOrWhiteSpace(newName)) return;
        PlayerName.Value = new FixedString64Bytes(newName.Trim());
    }

    //Seta o estado de ready do jogador. Só funciona se tiver skin válida.
    public void SetReady(bool ready)
    {
        if (!IsOwner) return;
        if (ready && !ValidSkin) return;
        IsReady.Value = ready;
    }

    //Cada owner inscreve o próprio IsLocked. LockAllProfiles já itera os perfis e chama em cada um.
    private void LockProfile()
    {
        if(!IsOwner) return;
        IsLocked.Value = true;
    }

    [Rpc(SendTo.Owner)]
    public void RequestLockRpc()
    {
        LockProfile();
    }

    //Aplica o visual localmete sempre que skinIndex muda. Aplica a sprite no SpriteRenderer.
    private void ApplyVisual(int skinIndex)
    {
        if (skinIndex < 0 || PersonagemRenderizado == null) return;
        if (SkinManager.Instance == null) return;

        //Mudança do visual na prática.
        Sprite skinSprite = SkinManager.Instance.GetSkinSprite(skinIndex);
        if (skinSprite != null)
            PersonagemRenderizado.sprite = skinSprite;
    }
}