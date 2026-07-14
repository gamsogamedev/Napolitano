using System.Collections.Generic;
using System.Linq;
using AudioSystem;
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

    [SerializeField] private SoundData switchSound;
    
    
    private bool toggleEnabled;
    
    
    [SerializeField] private List<NetworkTogglableItem> targetItem;

    private void Update()
    {
        if (!canRotate) return;
        if(toggleEnabled) transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
    
    public bool CanInteract(PlayerController interactor)
    {
        return (interactor.CurrentState == interactor.ConeState && canConeInteract) || 
               (interactor.CurrentState == interactor.IceCreamState && canIceCreamInteract);
    }

    public void Interact(PlayerController interactor)
    {
        toggleEnabled = !toggleEnabled;
        SoundManager.Instance.CreateSound().Play(switchSound);
        
        foreach(var item in targetItem) item.ToggleItem();
    }
}