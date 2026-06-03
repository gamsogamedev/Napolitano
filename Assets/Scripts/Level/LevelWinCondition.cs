using System;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;

public class LevelWinCondition : NetworkBehaviour, IInteractable
{
    
    private HashSet<ulong> playersWon = new HashSet<ulong>();
    
    public static event Action OnLevelComplete;

    // TODO: São 4 da manhã e eu não quero me dar o trabalho de entender essa porr
    public bool CanInteract(PlayerController interactor)
    {
        return true;
    }

    public void Interact(PlayerController interactor)
    {
        //TODO: adicionar um "DisableController: para o interactor
        //Isso disabilitaria o controle do jogador e removeria o player da cena, dps a gente faz um trigger pra rodar
        //uma animação na condição de vitoria do personagem entrando no cone de sorvete
        
        SubmitWinInteractionRPC(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Owner)]
    private void SubmitWinInteractionRPC(ulong clientId)
    {
        playersWon.Add(clientId);
        
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
    
}
