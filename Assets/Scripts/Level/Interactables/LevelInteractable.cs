using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.Netcode;
using UnityEngine;

public class LevelInteractable : NetworkBehaviour, IInteractable
{
    [SerializeField] private bool canConeInteract;
    [SerializeField] private bool canIceCreamInteract;
    
    [Header("Rotation")]
    [SerializeField] private bool canRotate = true;
    [SerializeField] private float rotationSpeed;
    
    private bool toggleEnabled;
    
    
    [SerializeField] private NetworkTogglableItem targetItem;

    private void Update()
    {
        if (!canRotate) return;
        if(toggleEnabled) transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
    
    public bool CanInteract(PlayerController interactor)
    {
        toggleEnabled = !toggleEnabled;
        
        return (interactor.CurrentState == interactor.ConeState && canConeInteract) || 
               (interactor.CurrentState == interactor.IceCreamState && canIceCreamInteract);
    }

    public void Interact(PlayerController interactor)
    {
        targetItem?.ToggleItem();
    }
}