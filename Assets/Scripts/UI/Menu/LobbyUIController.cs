using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Cysharp.Threading.Tasks;
using Player;
using Unity.Netcode;
using Unity.Loading;
using UI;

public class LobbyUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private CustomButton exitGameButton;
    [SerializeField] private TMP_InputField roomCodeInput;

    [Header("Host Buttons")]
    [SerializeField] private CustomButton createSessionButton;
    
    [Header("Client Buttons")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private CustomButton joinSessionButton;

    [Header("Nome da sua cena")]
    [SerializeField] private string gameSceneName;

    private bool sceneLoading;

    private NetworkManager networkManager;

    private void OnEnable()
    {
        createSessionButton.OnClicked += OnCreateSessionClicked;
        joinSessionButton.OnClicked += OnJoinSessionClicked;
        exitGameButton.OnClicked += OnExitGameClicked;
    }

    private void OnDisable()
    {
        createSessionButton.OnClicked -= OnCreateSessionClicked;
        joinSessionButton.OnClicked -= OnJoinSessionClicked;
        exitGameButton.OnClicked -= OnExitGameClicked;

        UnsubscribeNetworkEvents();
    }

    private void Start()
    {
        roomCodeInput.gameObject.SetActive(false);
        networkManager = NetworkManager.Singleton;
    }
    
    private void SetUIInteractable(bool isEnabled)
    {
        createSessionButton.Interactable = isEnabled;
        joinSessionButton.Interactable = isEnabled;
        joinCodeInput.interactable = isEnabled;
    }

    #region Buttons

        private async void OnCreateSessionClicked()
        {
            PlayerProfile profile = SessionManager.Instance.CurrentProfile;
            if(profile == null)
            {
                statusLabel.text = "Nenhum perfil carregado";
                return;
            }
            
            statusLabel.text = "Criando sessão...";
            SetUIInteractable(false);

            await SessionManager.Instance.CreateSessionAsHost(profile);

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

            PlayerProfile profile = SessionManager.Instance.CurrentProfile;
            if (profile == null)
            {
                statusLabel.text = "Nenhum perfil carregado";
                return;
            }

            statusLabel.text = "Joining Session...";
            SetUIInteractable(false);

            await SessionManager.Instance.JoinSessionByCode(code, profile);

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

    #endregion

    private void LoadGameScene()
    {
        if (sceneLoading) return;

        sceneLoading = true;

        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
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

                int currentPlayerCount = networkManager.ConnectedClientsIds.Count;

                if (currentPlayerCount == 2)
                {
                    statusLabel.text = "Player 2 conectado";
                    roomCodeInput.gameObject.SetActive(false);

                    LoadGameScene();
                }
                else
                {
                    statusLabel.text = "Code: ";
                    roomCodeInput.text = SessionManager.Instance.CurrentSessionCode;
                    roomCodeInput.gameObject.SetActive(true);
                }
            }
        }

    #endregion
}