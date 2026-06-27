using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[]  spawnPoints;

    [Header("Level Data")] 
    [SerializeField] private int levelNumber;
    
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SpawnPlayers;    
        }
        
        LevelWinCondition.OnLevelComplete += TriggerLevelComplete;
        PlayerCollision.OnPlayerTookDamage += TriggerLeveFail;
    }
    
    public override void OnNetworkDespawn()
    {
        LevelWinCondition.OnLevelComplete -= TriggerLevelComplete;
        PlayerCollision.OnPlayerTookDamage -= TriggerLeveFail;
    }

    private void SpawnPlayers(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SpawnPlayers;
        
        int playerCounter = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[playerCounter].position, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);

            playerCounter++;
        }
    }

    private void TriggerLevelComplete()
    {

        if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
        {
            ModalManager.Instance.ShowModal(new ModalDetails
            {
                title = "Level " +  levelNumber + " Concluído!",
                confirmText = "Voltar para seleção de mapa",
                onConfirm = async () =>
                {
                    await SessionManager.Instance.UpdateMaxLevel(levelNumber + 1);

                    NetworkManager.Singleton.SceneManager.LoadScene("LevelSelector", LoadSceneMode.Single);
                }
            });
        }
        else
        {
            ModalManager.Instance.ShowModal(new ModalDetails
            {
                title = "Level " +  levelNumber + " Concluído!",
                message = "Esperando o host...",
            });
        }
        
    }

    private void TriggerLeveFail(string playerName)
    {
        string customMessage = "Jogador " + playerName + " morreu!";

        if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
        {
            ModalManager.Instance.ShowModal(new ModalDetails
            {
                title = "Game Over",
                message = customMessage,
                confirmText = "Restart",
                onConfirm = () =>
                {
                    NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
                }
            });
        }
        else
        {
            ModalManager.Instance.ShowModal(new ModalDetails
                {
                    title = "Game Over",
                    message = customMessage + " Esperando o host...",
                }
            );
        }
    }
}
