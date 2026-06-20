using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Cysharp.Threading.Tasks;
using Player;
using Unity.Netcode; 

public class LobbyUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private TMP_InputField roomCodeInput;

    [Header("Host Buttons")]
    [SerializeField] private Button createSessionButton;
    [SerializeField] private Button continueGameButton;
    
    [Header("Client Buttons")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinSessionButton;
    
    [Header("Player Data")]
    [SerializeField] private TMP_InputField playerNameInput;
    
    [Header("Nome da sua cena")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    private PlayerSprite playerSprite = PlayerSprite.Strawberry;
    
    private NetworkManager networkManager;

    private void OnEnable()
    {
        createSessionButton.onClick.AddListener(OnCreateSessionClicked);
        joinSessionButton.onClick.AddListener(OnJoinSessionClicked);
        continueGameButton.onClick.AddListener(OnContinueGameClicked);
        exitGameButton.onClick.AddListener(OnExitGameClicked);
    }

    private void OnDisable()
    {
        createSessionButton.onClick.RemoveListener(OnCreateSessionClicked);
        joinSessionButton.onClick.RemoveListener(OnJoinSessionClicked);
        continueGameButton.onClick.RemoveListener(OnContinueGameClicked);
        exitGameButton.onClick.RemoveListener(OnExitGameClicked);

        UnsubscribeNetworkEvents();
    }

    private void Start()
    {
        continueGameButton.gameObject.SetActive(false);
        roomCodeInput.gameObject.SetActive(false);
        networkManager = NetworkManager.Singleton;
    }
    
    private void SetUIInteractable(bool isEnabled)
    {
        createSessionButton.interactable = isEnabled;
        joinSessionButton.interactable = isEnabled;
        joinCodeInput.interactable = isEnabled;
    }

    #region Buttons

        private async void OnCreateSessionClicked()
        {

            var playerName = playerNameInput.text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                statusLabel.text = "Escreva o seu nome para criar uma sala";
                return;
            }
            
            statusLabel.text = "Criando sessão...";
            SetUIInteractable(false);

            await SessionManager.Instance.CreateSessionAsHost(playerNameInput.text.Trim(), playerSprite);

            if (SessionManager.Instance.ActiveSession != null)
            {
                SubscribeNetworkEvents();
                
                RefreshLobbyUI(); 
            }
            else
            {
                statusLabel.text = "Falha ao criar sessão";
                SetUIInteractable(true);
            }
        }

        private async void OnJoinSessionClicked()
        {
            string code = joinCodeInput.text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                statusLabel.text = "Codigo invalido";
                return;
            }
            
            var playerName = playerNameInput.text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                statusLabel.text = "Escreva o seu nome para criar uma sala";
                return;
            }

            statusLabel.text = "Joining Session...";
            SetUIInteractable(false);

            await SessionManager.Instance.JoinSessionByCode(code, playerName, playerSprite);

            if (SessionManager.Instance.ActiveSession != null)
            {
                statusLabel.text = "Conectado em: " + SessionManager.Instance.ActiveSession.Code;
                
                SubscribeNetworkEvents();
            }
            else
            {
                statusLabel.text = "Falha ao conectar. Tente novamente.";
                SetUIInteractable(true);
            }
        }
        
        private void OnContinueGameClicked()
        {
            bool isSessionOwner = networkManager.LocalClient != null && networkManager.LocalClient.IsSessionOwner;

            if (isSessionOwner && networkManager.ConnectedClientsIds.Count == 2)
            {
                continueGameButton.interactable = false;
                networkManager.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        private async void OnExitGameClicked()
        {
            try
            {
                await SessionManager.Instance.LeaveSession();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void OnSkinDrowdownChanged(Int32 index)
        {
            switch (index)
            {
                case 0: playerSprite = PlayerSprite.Strawberry; break;
                case 1: playerSprite = PlayerSprite.Vanilla; break;
                case 2: playerSprite = PlayerSprite.Chocolate; break;
            }
        }

    #endregion
    
    #region Network Events
    
        private void SubscribeNetworkEvents()
        {
            if (networkManager == null) return;
            
            UnsubscribeNetworkEvents();
            networkManager.OnClientConnectedCallback += OnPlayerConnectionChanged;
            networkManager.OnClientDisconnectCallback += OnPlayerConnectionChanged;
        }

        private void UnsubscribeNetworkEvents()
        {
            if (networkManager == null) return;
            
            networkManager.OnClientConnectedCallback -= OnPlayerConnectionChanged;
            networkManager.OnClientDisconnectCallback -= OnPlayerConnectionChanged;
        }

        private void OnPlayerConnectionChanged(ulong clientId)
        {
            RefreshLobbyUI();
        }

        private void RefreshLobbyUI()
        {
            if (networkManager == null || !networkManager.IsListening) return;

            bool isSessionOwner = networkManager.LocalClient != null && networkManager.LocalClient.IsSessionOwner;

            if (isSessionOwner)
            {
                continueGameButton.gameObject.SetActive(true);

                int currentPlayerCount = networkManager.ConnectedClientsIds.Count;

                continueGameButton.interactable = (currentPlayerCount == 2);

                if (currentPlayerCount == 2)
                {
                    statusLabel.text = "Player 2 conectado";
                    roomCodeInput.gameObject.SetActive(false);
            }
                else
                {
                    statusLabel.text = "Code: ";
                    roomCodeInput.text = SessionManager.Instance.CurrentSessionCode;
                    roomCodeInput.gameObject.SetActive(true);
                }
            }
            else
            {
                continueGameButton.gameObject.SetActive(false);
            }
        }

    #endregion
}