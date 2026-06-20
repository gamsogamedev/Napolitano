using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] CharacterDataBase dataBase;

    [Header("Panels")]
    [SerializeField] CharacterSelectionPanel sessionOwnerPanel;
    [SerializeField] CharacterSelectionPanel clientPanel;

    [Header("Buttons")]
    [SerializeField] private Button startGameButton;

    [Header("Next scene")]
    [SerializeField] private string gameSceneName;

    private int sessionOwnerPrevIndex = 0;
    private int clientPrevIndex = 0;
    
    private void Start()
    {
        if (SessionManager.Instance.ActiveSession != null)
        {
            SessionManager.Instance.ActiveSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
        }
    }

    #region RegisterEvents

        private void OnEnable()
        {
            RegisterEvents();
                
            startGameButton.onClick.AddListener(OnStartGameClicked);
                
            InitializeUI();
        }

        private void OnDisable()
        {
            startGameButton.onClick.RemoveListener(OnStartGameClicked);

            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            sessionOwnerPanel.OnPreviusPressed += OnSessionOwnerPreviusPressed;
            sessionOwnerPanel.OnNextPressed += OnSessionOwnerNextPressed;
            sessionOwnerPanel.OnSelectPressed += OnSessionOwnerSelectPressed;

            clientPanel.OnPreviusPressed += OnClientPreviusPressed;
            clientPanel.OnNextPressed += OnClientNextPressed;
            clientPanel.OnSelectPressed += OnClientSelectPressed;
        }

        private void UnregisterEvents()
        {
            sessionOwnerPanel.OnPreviusPressed -= OnSessionOwnerPreviusPressed;
            sessionOwnerPanel.OnNextPressed -= OnSessionOwnerNextPressed;
            sessionOwnerPanel.OnSelectPressed -= OnSessionOwnerSelectPressed;

            clientPanel.OnPreviusPressed -= OnClientPreviusPressed;
            clientPanel.OnNextPressed -= OnClientNextPressed;
            clientPanel.OnSelectPressed -= OnClientSelectPressed;
        }

    #endregion

    private void OnPlayerPropertiesChanged()
    {
        Debug.Log("Trocou propriedade");
        LoadRemotePlayerSelection();
    }
    
    private void LoadRemotePlayerSelection()
    {
        if (SessionManager.Instance.ActiveSession == null)
            return;

        foreach (var player in SessionManager.Instance.ActiveSession.Players)
        {
            if (!player.Properties.TryGetValue(SessionManager.playerSkinPropertyKey, out var property))
                continue;
            
            int index = property.Value == nameof(PlayerSprite.Strawberry) ? 0 : 1;
            
            Debug.Log(index);

            // ignora eu mesmo
            if (player.Id != SessionManager.Instance.ActiveSession.CurrentPlayer.Id)
            {
                Character character = dataBase.GetCharacter(index);

                bool isHost = NetworkManager.Singleton.LocalClient.IsSessionOwner;

                if (isHost)
                {
                    // host vê a escolha do cliente
                    clientPanel.SetCharacter(character);
                }
                else
                {
                    // cliente vê a escolha do host
                    sessionOwnerPanel.SetCharacter(character);
                }
            }
        }
    }

    #region ButtonBehaviour

        private void OnSessionOwnerPreviusPressed()
        {
            sessionOwnerPrevIndex--;
            if (sessionOwnerPrevIndex < 0)
            {
                sessionOwnerPrevIndex = dataBase.characterCount - 1;
            }

            Character character = dataBase.GetCharacter(sessionOwnerPrevIndex);

            sessionOwnerPanel.SetCharacter(character);
        }

        private void OnSessionOwnerNextPressed()
        {
            sessionOwnerPrevIndex++;
            if (sessionOwnerPrevIndex >= dataBase.characterCount)
            {
                sessionOwnerPrevIndex = 0;
            }

            Character character = dataBase.GetCharacter(sessionOwnerPrevIndex);

            sessionOwnerPanel.SetCharacter(character);
        }

        private async void OnSessionOwnerSelectPressed()
        {

            await SessionManager.Instance.UpdateSelectedCharacter(sessionOwnerPrevIndex);

            sessionOwnerPanel.SetInteractable(false);

            CheckStartButton();
        }

        private void OnClientPreviusPressed()
        {
            clientPrevIndex--;
            if (clientPrevIndex < 0)
            {
                clientPrevIndex = dataBase.characterCount - 1;
            }

            Character character = dataBase.GetCharacter(clientPrevIndex);

            clientPanel.SetCharacter(character);
        }

        private void OnClientNextPressed()
        {
            clientPrevIndex++;
            if (clientPrevIndex >= dataBase.characterCount)
            {
                clientPrevIndex = 0;
            }

            Character character = dataBase.GetCharacter(clientPrevIndex);

            clientPanel.SetCharacter(character);
        }

        private async void OnClientSelectPressed()
        {

            await SessionManager.Instance.UpdateSelectedCharacter(clientPrevIndex);

            sessionOwnerPanel.SetInteractable(false);

            CheckStartButton();
        }

    #endregion

    private void InitializeUI()
    {
        if (dataBase == null)
        {
            Debug.LogError("CharacterDatabase não foi atribuída.");
            return;
        }
        if (sessionOwnerPanel == null)
        {
            Debug.LogError("SessionOwnerPanel não atribuído.");
            return;
        }
        if (clientPanel == null)
        {
            Debug.LogError("ClientPanel não atribuído.");
            return;
        }
        
        //sessionOwnerPanel.SetCharacter(dataBase.GetCharacter(sessionOwnerPrevIndex));
        //clientPanel.SetCharacter(dataBase.GetCharacter(clientPrevIndex));

        InitializePlayerNames();

        ConfigureLocalPermissions();

        LoadRemotePlayerSelection();
    }

    private void CheckStartButton()
    {
        if (SessionManager.Instance.ActiveSession == null)
            return;

        bool hostSelected = false;
        bool clientSelected = false;

        foreach (var player in SessionManager.Instance.ActiveSession.Players)
        {
            if (!player.Properties.TryGetValue(SessionManager.playerSkinPropertyKey, out var property))
                continue;
            
            int index = property.Value == nameof(PlayerSprite.Strawberry) ? 0 : 1;
            
            Debug.Log(index);

            if (player.Id == SessionManager.Instance.ActiveSession.CurrentPlayer.Id)
            {
                // minha escolha
                if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
                    hostSelected = true;
                else
                    clientSelected = true;
            }
            else
            {
                // escolha do outro jogador
                if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
                    clientSelected = true;
                else
                    hostSelected = true;
            }
        }

        startGameButton.interactable = hostSelected && clientSelected;
    }

    private void InitializePlayerNames()
    {
        if (SessionManager.Instance.ActiveSession == null) return;

        foreach (var player in SessionManager.Instance.ActiveSession.Players)
        {
            if (!player.Properties.TryGetValue("playerName", out var property))
                continue;

            string playerName = property.Value;

            if (player.Id == SessionManager.Instance.ActiveSession.CurrentPlayer.Id)
            {
                // jogador local
                if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
                {
                    sessionOwnerPanel.SetPlayerName(playerName);
                }
                else
                {
                    clientPanel.SetPlayerName(playerName);
                }
            }
            else
            {
                // outro jogador
                if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
                {
                    clientPanel.SetPlayerName(playerName);
                }
                else
                {
                    sessionOwnerPanel.SetPlayerName(playerName);
                }
            }
        }
    }

    private void ConfigureLocalPermissions()
    {
        bool isSessionOwner = NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.IsSessionOwner;

        sessionOwnerPanel.SetInteractable(isSessionOwner);
        clientPanel.SetInteractable(!isSessionOwner);

        startGameButton.gameObject.SetActive(isSessionOwner);
        startGameButton.interactable = false;
    }

    private void OnStartGameClicked()
    {
        bool isSessionOwner = NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.IsSessionOwner;

        if (isSessionOwner && NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            startGameButton.interactable = false;
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
