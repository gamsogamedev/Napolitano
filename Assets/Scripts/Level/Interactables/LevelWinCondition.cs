using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Player;
using Unity.Netcode;
using UnityEngine;

public class LevelWinCondition : NetworkBehaviour, IInteractable
{

    [SerializeField] private SpriteRenderer finishLine;
    
    [SerializeField] private Sprite completedSprite;
    
    
    
    private HashSet<ulong> playersWon = new HashSet<ulong>();
    
    public static event Action OnLevelComplete;
    
    public bool CanInteract(PlayerController interactor)
    {
        return true;
    }

    public void Interact(PlayerController interactor)
    {
        interactor.DisableController(true);
        
        finishLine.sprite = completedSprite;

        SubmitWinInteractionRPC(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Owner)]
    private void SubmitWinInteractionRPC(ulong clientId)
    {
        playersWon.Add(clientId);

        DisableWinningPlayerRpc(clientId);

        if (playersWon.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            TriggerLevelCompleteRPC();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void TriggerLevelCompleteRPC()
    {
        OnLevelComplete?.Invoke();
    }

    [Rpc(SendTo.Everyone)]
    private void DisableWinningPlayerRpc(ulong clientId) {
        if (PlayerController.AllPlayers.TryGetValue(clientId, out var player)) {
            player.DisableController(true);
        }
    }
}
