using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Player;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityUtils;

public class SessionManager : Singleton<SessionManager>
{
    public event Action OnSelfConnectionLost;
    public event Action OnTeammateConnectionLost;
    
    private ISession activeSession;
    public ISession ActiveSession
    {
        get => activeSession;
        private set
        {
            activeSession = value;
            Debug.Log("Active Session: " + (activeSession != null ? activeSession.Id : "None"));
        }
    }

    public string CurrentSessionCode => ActiveSession?.Code; 

    const string playerNamePropertyKey = "playerName";
    const string playerSkinPropertyKey = "playerSkin";
    const string playerMaxLevelPropertyKey = "playerMaxLevel";
    const string playerSelectedCharacterKey = "selectedCharacter";

    public string LocalPlayerName {  get; private set; }
    public PlayerSprite LocalPlayerSprite {  get; private set; }
    
    private NetworkManager networkManager;
    
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("AuthenticationService Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async UniTask UpdatePlayerData(string playerName, PlayerSprite sprite)
    {
        LocalPlayerName = playerName;
        LocalPlayerSprite = sprite;

        var properties = await GetPlayerPropertiesAsyncWithName(playerName, sprite);

        ActiveSession.CurrentPlayer.SetProperties(properties);

        await ActiveSession.SaveCurrentPlayerDataAsync();
    }

    public async UniTask UpdateSelectedCharacter(int characterIndex)
    {
        if (ActiveSession == null) { return; }

        var property = new PlayerProperty(characterIndex.ToString(),VisibilityPropertyOptions.Member);

        ActiveSession.CurrentPlayer.SetProperty(playerSelectedCharacterKey,property);

        await ActiveSession.SaveCurrentPlayerDataAsync();
    }

    async UniTask<Dictionary<string, PlayerProperty>> GetPlayerPropertiesAsync()
    {
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        
        return new Dictionary<string, PlayerProperty>{{playerNamePropertyKey, playerNameProperty}};
    }

    async UniTask<Dictionary<string, PlayerProperty>> GetPlayerPropertiesAsyncWithName(string playerName, PlayerSprite playerSprite)
    {
        
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        var playerSkinProperty = new PlayerProperty(playerSprite.ToString(), VisibilityPropertyOptions.Member);
        await UniTask.CompletedTask;
        
        return new Dictionary<string, PlayerProperty>
        {
            {playerNamePropertyKey, playerNameProperty},
            {playerSkinPropertyKey, playerSkinProperty}
        };
    }

    public string GetLocalPlayerName()
    {
        return LocalPlayerName;
    }

    public PlayerSprite GetLocalPlayerSprite()
    {
        return LocalPlayerSprite;
    }

    public async UniTask CreateSessionAsHost(string playerName, PlayerSprite sprite) 
    {
        try 
        {
            var playerProperties = await GetPlayerPropertiesAsyncWithName(playerName, sprite);
            LocalPlayerName = playerName;
            LocalPlayerSprite = sprite;
            
            var options = new SessionOptions()
            {
                MaxPlayers = 2,
                IsLocked = false,
                IsPrivate = false,
                PlayerProperties = playerProperties
            }.WithDistributedAuthorityNetwork();
            
            ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            SubscribeToEvents();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create session: {e.Message}");
        }
    }

    public async UniTask JoinSessionByCode(string code, string playerName, PlayerSprite sprite) 
    {
        try 
        {
            var playerProperties = await GetPlayerPropertiesAsyncWithName(playerName, sprite);
            LocalPlayerName = playerName;
            LocalPlayerSprite = sprite;

            var joinOptions = new JoinSessionOptions()
            {
                PlayerProperties = playerProperties
            };

            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code, joinOptions);
            SubscribeToEvents();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join session: {e.Message}");
        }
    }
    
    private void SubscribeToEvents()
    {
        if (ActiveSession != null)
        {
            ActiveSession.PlayerLeaving += HandlePlayerLeft;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleLocalDisconnect;
            }
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (ActiveSession != null)
        {
            ActiveSession.PlayerLeaving -= HandlePlayerLeft;
        }
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleLocalDisconnect;
        }
    }
    
    private void HandlePlayerLeft(string playerId)
    {
        OnTeammateConnectionLost?.Invoke();
    }
    
    private void HandleLocalDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("SessionManager: Self connection lost.");
            OnSelfConnectionLost?.Invoke();
            
            UnsubscribeFromEvents();
            ActiveSession = null; 
        }
    }

    //TODO: Implementar no lobby
    public async UniTask KickPlayer(string playerID)
    {
        if (!ActiveSession.IsHost) return;
        
        await ActiveSession.AsHost().RemovePlayerAsync(playerID);
    }

    //TODO: Implementar no lobby DECENTEMENTE
    public async UniTask LeaveSession()
    {
        if (ActiveSession != null)
        {
            try
            {
                await ActiveSession.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                UnsubscribeFromEvents();
                ActiveSession = null;
                
                if (NetworkManager.Singleton != null)
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }
        }
    }
}