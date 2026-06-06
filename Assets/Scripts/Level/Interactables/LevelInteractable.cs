using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.Netcode;
using UnityEngine;

public class LevelInteractable : NetworkBehaviour, IInteractable
{
    [SerializeField] private bool canConeInteract;
    [SerializeField] private bool canIceCreamInteract;
    
    [SerializeField] private NetworkTogglableItem targetItem; 
    
    public bool CanInteract(PlayerController interactor)
    {
        return (interactor.CurrentState == interactor.ConeState && canConeInteract) || 
               (interactor.CurrentState == interactor.IceCreamState && canIceCreamInteract);
    }

    public void Interact(PlayerController interactor)
    {
        targetItem?.ToggleItem(); 
    }
}