using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System.Runtime.CompilerServices;
using Blocks.Sessions.Common;
using System.Threading.Tasks;

//Controla os paineis do lobby
//  - Painel Inicial: criar/entrar na sessão
//  - Painel Sala: aguarar os dois jogadores, mostra status, ready, start e saída

//Dependências esperadas na cena
//  - SessionManager.Instance -> gerencia sessão de rede
//  - SkinManager.Instance -> sprites de skin
//  - NetworkManager.Singleton -> callbacks de rede
//  - PlayerProfileDados -> no PlayerObject de cada cliente

public class LobbyUITeste : MonoBehaviour
{
    //----------------------------------------------------------------
    //PAINÉIS
    //----------------------------------------------------------------
    [Header("Painéis")]
    [SerializeField] private GameObject initialPanel;
    [SerializeField] private GameObject salaPanel;
    [SerializeField] private GameObject mapSelectorPanel;

    //----------------------------------------------------------------
    //PAINEL INICIAL
    //----------------------------------------------------------------
    [Header("Painel Inicial")]
    [SerializeField] private Button createSession;
    [SerializeField] private TMP_InputField joinCode;
    [SerializeField] private Button joinSession;
    [SerializeField] private TextMeshProUGUI initialstatusLabel;

    //----------------------------------------------------------------
    //PAINEL SALA
    //----------------------------------------------------------------
    [Header("Painel Sala - Player 1")]
    [SerializeField] private Image player1SkinPreview;
    [SerializeField] private TextMeshProUGUI player1NameLabel;
    [SerializeField] private TextMeshProUGUI player1ReadyLabel;
    [SerializeField] private GameObject player1EmptySlot;
    [SerializeField] private Button readyPlayer1;

    [Header("Painel Sala - Player 2")]
    [SerializeField] private Image player2SkinPreview;
    [SerializeField] private TextMeshProUGUI player2NameLabel;
    [SerializeField] private TextMeshProUGUI player2ReadyLabel;
    [SerializeField] private GameObject player2EmptySlot;
    [SerializeField] private Button readyPlayer2;

    [Header("Painel Sala - Controles")]
    [SerializeField] private TextMeshProUGUI codigoSalaLabel;
    [SerializeField] private Button openPlayerProfile;
    [SerializeField] private Button start;
    [SerializeField] private Button leaveSala;
    [SerializeField] private TextMeshProUGUI salaStatusLabel;

    [Header("Nome da sua cena")]
    [SerializeField] private string gameSceneName = "GameScene";

    
    //----------------------------------------------------------------
    //ESTADO INTERNO
    //----------------------------------------------------------------
    private NetworkManager networkManager;

    //Referencias aos perfis dos dois jogadores (populadas dinamicamente). Começam null e são preenchidas quando os objetos de rede aparecem.
    private PlayerProfileDados localProfile;
    private PlayerProfileDados remoteProfile;

    //Guarda perfins já inscritos, para não inscrever duas vezes.
    private readonly HashSet<ulong> subscribedProfiles = new HashSet<ulong>();


    //----------------------------------------------------------------
    //CICLO DE VIDA UNITY
    //----------------------------------------------------------------
    //Inicia no painel inicial.
    private void Awake()
    {
        ShowPanel(initialPanel);
    }

    //O onClick é um evento do botão, quando acontece algo com ele chama uma lista de funções, a função chamada é adicionada pelo AddListener.
    private void OnEnable()
    {
        createSession.onClick.AddListener(OnCreateSessionClicked);
        joinSession.onClick.AddListener(OnJoinSessionClicked);
        readyPlayer1.onClick.AddListener(() => OnReadyClicked(isPlayer1Slot: true));
        readyPlayer2.onClick.AddListener(() => OnReadyClicked(isPlayer1Slot: false));
        start.onClick.AddListener(OnStartClicked);
        leaveSala.onClick.AddListener(OnLeaveSalaClicked);
        SessionManager.Instance.OnSessionEnded += HandleSessionEnded;
    }

    //Quando o objeto é desativado, o remove a função da lista.
    //Evita memory leak. Se não remover o botão continua com referência à função, mesmo com o objeto destruído, causando erros e consumo de memória.
    private void OnDisable()
    {
        createSession.onClick.RemoveListener(OnCreateSessionClicked);
        joinSession.onClick.RemoveListener(OnJoinSessionClicked);
        readyPlayer1.onClick.RemoveAllListeners();
        readyPlayer2.onClick.RemoveAllListeners();
        start.onClick.RemoveListener(OnStartClicked);
        leaveSala.onClick.RemoveListener(OnLeaveSalaClicked);

        UnsubscribeNetworkEvents();

        if(SessionManager.Instance != null)
            SessionManager.Instance.OnSessionEnded -= HandleSessionEnded;
    }

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        start.gameObject.SetActive(false);
    }

    //Mudança de painel. Garantindo apenas um painel ativo por vez, pelo resultado bool do comparativo das variáveis paineis com o panelAlvo, sempre dando true para apenas um.
    private void ShowPanel(GameObject panelAlvo)
    {
        if (initialPanel != null) initialPanel.SetActive(initialPanel == panelAlvo);
        if (salaPanel != null) salaPanel.SetActive(salaPanel == panelAlvo);
        if (mapSelectorPanel != null) mapSelectorPanel.SetActive(mapSelectorPanel == panelAlvo);
    }


    //----------------------------------------------------------------
    //PAINEL INICIAL - BOTÕES 
    //----------------------------------------------------------------

    //Cria a sessão.
    //Método assíncrono, ou seja, pode ser pausado no meio com await sem travar o jogo.
    private async void OnCreateSessionClicked()
    {
        initialstatusLabel.text = "Criando sessão...";
        SetInitialUIInteractable(false);

        //Espera o SessionManager criar a sessão na rede. Pausa o método até operação terminar, mas não pausa a Unity.
        await SessionManager.Instance.CreateSessionAsHost();

        //Verifica se a sala foi criada.
        if(SessionManager.Instance.ActiveSession != null)
        {
            EnterRoom();
        }
        else
        {
            initialstatusLabel.text = "Falha ao criar sessão";
            SetInitialUIInteractable(true);
        }
    }

    //Entrada em sessão já existente por código.
    private async void OnJoinSessionClicked()
    {
        string code = joinCode.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            initialstatusLabel.text = "Código inválido";
            return;
        }

        initialstatusLabel.text = "Entrando na sessão";
        SetInitialUIInteractable(false);

        //Espera o jogador ser conectado a sessão. Nesse código permite que clique no join mesmo sem código escrito.
        await SessionManager.Instance.JoinSessionByCode(code);

        //Verifica se conseguiu entrar.
        if(SessionManager.Instance.ActiveSession != null)
        {
            EnterRoom();
        }
        else
        {
            initialstatusLabel.text = "Falha ao conectar. Tente novamente";
            SetInitialUIInteractable(true);
        }
    }

    //Pelo interavtable permite habilitar clique no botão. Botão cinza sem responder a cliques se false, e o contrário se true.
    private void SetInitialUIInteractable(bool habilitado)
    {
        createSession.interactable = habilitado;
        joinSession.interactable = habilitado;
        joinCode.interactable = habilitado;
    }


    //----------------------------------------------------------------
    //TRANSIÇÃO PARA O PAINEL SALA
    //----------------------------------------------------------------
    
    private void EnterRoom()
    {
        SubscribeNetworkEvents();
        ShowPanel(salaPanel);

        //Inicia coroutine que aguarda o PlayerObject local aparecer na rede.
        //Courotine é uma função especial que pode pausar e continuar, o Unity roda ela em paralelo com o resto do jogo.
        StartCoroutine(WaitForLocalProfileAndRefresh());
    }

    //Aguarda perfil local para Refresh na tela local quando entra na sala.
    //IEnumerator é o tipo de toda coroutine.
    private IEnumerator WaitForLocalProfileAndRefresh()
    {
        while(localProfile == null)
        {
            TryFindProfiles();
            yield return new WaitForSeconds(0.2f); //Pausa da coroutine, sem ele é um loop infinito.
        }
        RefreshSalaUI();
    }

    //Descoberta de perfis.
    //Percorre os clientes conectados, identifica-os e inscreve nos eventos deles. Chamado no loop de espera e nos callbacks da conexão.
    private void TryFindProfiles()
    {
        if (networkManager == null || !networkManager.IsListening) return;

        //ConnectedClients é um dicionário.
        //  kvp -> key-value pair:
        //      .Key -> clientId
        //      .Value -> dados do cliente
        foreach(var kvp in networkManager.ConnectedClients) //var deixa o compilador deduzir o tipo automaticamente.
        {
            var profile = kvp.Value.PlayerObject?.GetComponent<PlayerProfileDados>();
            if (profile == null) continue;

            bool isLocal = kvp.Key == networkManager.LocalClientId; //Variável que indica se a chave do client conectado é igual ao ID do cliente local.

            if (isLocal && localProfile == null)
                localProfile = profile;
            else if (!isLocal && remoteProfile == null)
                remoteProfile = profile;

            //Inscreve nos eventos do perfil uma única vez
            if (!subscribedProfiles.Contains(kvp.Key))
            {
                subscribedProfiles.Add(kvp.Key);
                profile.OnProfileChanged += RefreshSalaUI;
            }
        }
    }



    //----------------------------------------------------------------
    //PAINEL SALA - BOTÕES
    //----------------------------------------------------------------

    private void OnReadyClicked(bool isPlayer1Slot)
    {
        if (localProfile == null) return;

        bool localIsHost = IsLocalPlayerHost();
        bool slotIsForLocal = (isPlayer1Slot && localIsHost) || (!isPlayer1Slot && !localIsHost);

        if (!slotIsForLocal) return;

        bool newReady = !localProfile.IsReady.Value;
        localProfile.SetReady(newReady);
    }

    private void OnStartClicked()
    {
        if (networkManager == null) return;

        bool isSessionOwner = networkManager.LocalClient != null && networkManager.LocalClient.IsSessionOwner;

        if (!isSessionOwner) return;
        if (!AllPlayersReady()) return;

        //Trava os perfis antes de proseguir. Devo passar para os estados depois????????
        LockAllProfiles();

        //Evita double clicks.
        start.interactable = false;
        readyPlayer1.interactable = false;
        readyPlayer2.interactable = false;

        ShowPanel(mapSelectorPanel);

        //networkManager.SceneManager.LoadScene("MapSelectorScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnLeaveSalaClicked()
    {
        LeaveSala();
    }


    //----------------------------------------------------------------
    //SAÍDA DA SALA
    //----------------------------------------------------------------
    //Se for host, desconecta a sessão inteira. Todos os clientes recebem callback OnClienteDisconnect.
    //Se for cliente, apenas desconecta a si mesmo.
    private async void LeaveSala()
    {
        UnsubscribeAllProfileEvents();
        UnsubscribeNetworkEvents();
        ResetProfileReferences();
        networkManager.Shutdown();
        await SessionManager.Instance.LeaveSession();
        ReturnToInitialPanel();
    }

    //Retorna ao painel inicial.
    private void ReturnToInitialPanel()
    {
        initialstatusLabel.text = "";
        SetInitialUIInteractable(true);
        ShowPanel(initialPanel);
        joinCode.text = "";
    }

    //----------------------------------------------------------------
    //ATUALIZAÇÃO UI DA SALA
    //----------------------------------------------------------------
    private void RefreshSalaUI()
    {
        if (networkManager == null || !networkManager.IsListening) return;

        //Tenta encontrar perfis novos a cada Refresh, para o caso de cliente entrar tarde.
        TryFindProfiles();

        RefreshSalaCode();
        RefreshPlayerSlots();
        RefreshReadyButtons();
        RefreshStartButton();
        RefreshSalaStatus();
    }

    private void RefreshSalaCode()
    {
        if (codigoSalaLabel == null) return;

        string code = SessionManager.Instance?.CurrentSessionCode ?? "-";
        codigoSalaLabel.text = $"Código {code}";
    }

    private void RefreshPlayerSlots()
    {
        //Determina slot1 = host e slot2 = cliente.
        PlayerProfileDados hostProfile = GetHostProfile();
        PlayerProfileDados clientProfile = GetClientProfile();

        RefreshSingleSlot(
            profile: hostProfile,
            emptySlot: player1EmptySlot,
            skinImage: player1SkinPreview,
            nameLabel: player1NameLabel,
            readyLabel: player1ReadyLabel
        );

        RefreshSingleSlot(
            profile: clientProfile,
            emptySlot: player2EmptySlot,
            skinImage: player2SkinPreview,
            nameLabel: player2NameLabel,
            readyLabel: player2ReadyLabel
        );
    }

    private void RefreshSingleSlot(
        PlayerProfileDados profile,
        GameObject emptySlot,
        Image skinImage,
        TextMeshProUGUI nameLabel,
        TextMeshProUGUI readyLabel)
    {
        bool hasPlayer = profile != null;

        if (emptySlot != null) emptySlot.SetActive(!hasPlayer);
        if (skinImage != null) skinImage.gameObject.SetActive(hasPlayer);
        if (nameLabel != null) nameLabel.gameObject.SetActive(hasPlayer);
        if (readyLabel != null) readyLabel.gameObject.SetActive(hasPlayer);

        if (!hasPlayer) return;

        //SkinPreview
        if(skinImage != null)
        {
            Sprite sp = SkinManager.Instance?.GetSkinSprite(profile.SkinIndex.Value);
            if(sp != null)
                skinImage.sprite = sp;
            //Se sp é null usa sprite padrão do prefab.
        }

        //Nome
        if (nameLabel != null)
            nameLabel.text = profile.PlayerName.Value.ToString();

        //Ready
        if(readyLabel != null)
        {
            readyLabel.text = profile.IsReady.Value ? "PRONTO" : "AGUARDANDO...";
            readyLabel.color = profile.IsReady.Value ? Color.green : Color.gray;
        }
    }

    private void RefreshReadyButtons()
    {
        bool LocalIsHost = IsLocalPlayerHost();

        if (readyPlayer1 != null && localProfile != null)
        {
            bool isMyButton = LocalIsHost;
            bool validSkin = LocalIsHost ? localProfile.ValidSkin : false;
            bool isLocked = LocalIsHost ? localProfile.IsLocked.Value : false;
            bool hostHasSlot = GetHostProfile() != null;

            readyPlayer1.gameObject.SetActive(hostHasSlot);
            readyPlayer1.interactable = isMyButton && validSkin && isLocked;
        }
        if(readyPlayer2 != null && localProfile!= null)
        {
            bool isMyButton = !LocalIsHost;
            bool validSkin = !LocalIsHost ? localProfile.ValidSkin : false;
            bool isLocked = !LocalIsHost ? localProfile.IsLocked.Value : false;
            bool clientHasSlot = GetClientProfile() != null;

            readyPlayer2.gameObject.SetActive(clientHasSlot);
            readyPlayer2.interactable = isMyButton && validSkin && isLocked;
        }
    }

    private void RefreshStartButton()
    {
        if (start == null) return;

        bool isHost = IsLocalPlayerHost();
        bool AllReady = AllPlayersReady();

        //Apenas aparece para o host.
        start.gameObject.SetActive(isHost);

        if (isHost)
            start.interactable = AllReady;
    }

    private void RefreshSalaStatus()
    {
        if (salaStatusLabel == null) return;

        int count = networkManager.ConnectedClientsIds.Count;

        if(count < 2)
        {
            salaStatusLabel.text = "Aguardando jogador";
        }
        else if (!AllPlayersReady())
        {
            salaStatusLabel.text = IsLocalPlayerHost()? "Aguardando jogadores ficarem prontos" : "Aguardando todos prontos";
        }
        else
        {
            salaStatusLabel.text = IsLocalPlayerHost() ? "Todos Prontos" : "Todos prontos, aguardando host iniciar";
        }
    }


    //----------------------------------------------------------------
    //HELPERS
    //----------------------------------------------------------------
    //Se o player local é dono da sala ele é o host.
    private bool IsLocalPlayerHost()
    {
        return networkManager != null && networkManager.LocalClient != null && networkManager.LocalClient.IsSessionOwner;
    }

    //Verifica se todos os players estão prontos.
    private bool AllPlayersReady()
    {
        if (networkManager == null || networkManager.ConnectedClientsIds.Count < 2) return false;

        foreach(var kvp in networkManager.ConnectedClients)
        {
            var profile = kvp.Value.PlayerObject?.GetComponent<PlayerProfileDados>();
            if (profile == null || !profile.IsReady.Value) return false;
        }
        return true;
    }

    private PlayerProfileDados GetHostProfile()
    {
        foreach(var kvp in networkManager.ConnectedClients)
        {
            var profile = kvp.Value.PlayerObject?.GetComponent<PlayerProfileDados>();
            if (profile == null) continue;

            if(kvp.Key == networkManager.CurrentSessionOwner)
            {
                return profile;
            }
        }
        return null;
    }

    private PlayerProfileDados GetClientProfile()
    {
        foreach(var kvp in networkManager.ConnectedClients)
        {
            var profile = kvp.Value.PlayerObject?.GetComponent<PlayerProfileDados>();
            if (profile == null) continue;
            if(kvp.Key != networkManager.CurrentSessionOwner)
            {
                return profile;
            }
        }
        return null;
    }

    //Trava os perfis de todos os jogadores antes de ir para o jogo/MapSelector.
    //Chamado pelo servidor/host, após clicar em start.
    private void LockAllProfiles()
    {
        localProfile?.RequestLockRpc();
        remoteProfile?.RequestLockRpc();
    }


    //----------------------------------------------------------------
    //EVENTOS DE REDE
    //----------------------------------------------------------------
    //Eventos são vantajosos pois a cada mudança é possível só increver outra função no evento, ao invés de mudar o código.
    private void SubscribeNetworkEvents()
    {
        if (networkManager == null) return;
        UnsubscribeNetworkEvents(); //Segurança.

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void UnsubscribeNetworkEvents()
    {
        if (networkManager == null) return;
        
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    //Quando o novo jogador entra tenta descobrir o perfil dele e atualizar UI.
    private void OnClientConnected(ulong clientId)
    {
        TryFindProfiles();
        RefreshSalaUI();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        //Remove inscrição do perfil que saiu.
        if (subscribedProfiles.Contains(clientId))
        {
            subscribedProfiles.Remove(clientId);

            if (localProfile != null && localProfile.OwnerClientId == clientId)
                localProfile = null;
            if (remoteProfile != null && remoteProfile.OwnerClientId == clientId)
                remoteProfile = null;
        }

        //Se o host desconectou e sou cliente, volta ao painel inicial.
        if(clientId == networkManager.CurrentSessionOwner && !IsLocalPlayerHost())
        {
            HandleHostLeft();
            return;
        }

        RefreshSalaUI();
    }

    private async void HandleHostLeft()
    {
        UnsubscribeAllProfileEvents();
        UnsubscribeNetworkEvents();
        ResetProfileReferences();

        networkManager.Shutdown();

        await SessionManager.Instance.LeaveSession();
        ReturnToInitialPanel();
    }

    private void HandleSessionEnded()
    {
        initialstatusLabel.text = "Sessão encerrada";
        SetInitialUIInteractable(true);
        ShowPanel(initialPanel);
        joinCode.text = "";
    }


    //----------------------------------------------------------------
    //LIMPEZA DE REFERÊNCIAS
    //----------------------------------------------------------------
    //Limpeza da memória.
    private void UnsubscribeAllProfileEvents()
    {
        if (networkManager == null) return;

        foreach(var kvp in networkManager.ConnectedClients)
        {
            var profile = kvp.Value.PlayerObject?.GetComponent<PlayerProfileDados>();
            if (profile != null)
                profile.OnProfileChanged -= RefreshSalaUI;
        }
    }

    //Clear() esvazia o HashSet, evita que referências fiquem na memória apontando para objetos que não existem antes do Shutdown.
    private void ResetProfileReferences()
    {
        localProfile = null;
        remoteProfile = null;
        subscribedProfiles.Clear();
    }
}