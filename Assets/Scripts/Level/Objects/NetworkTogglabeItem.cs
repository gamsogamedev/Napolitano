using Unity.Netcode;
using UnityEngine;

public class NetworkTogglableItem : NetworkBehaviour
{
    [SerializeField] private Renderer sprite;
    [SerializeField] private Collider2D collision;
    [SerializeField] private bool startActive;
    
    private readonly NetworkVariable<bool> isItemActive = new (
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        ApplyVisualState(startActive);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) isItemActive.Value = startActive;

        isItemActive.OnValueChanged += HandleStateChange;
        ApplyVisualState(isItemActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isItemActive.OnValueChanged -= HandleStateChange;
    }
    
    public void ToggleItem()
    {
        RequestToggleRpc();
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestToggleRpc()
    {
        isItemActive.Value = !isItemActive.Value;
    }
    
    private void HandleStateChange(bool previousState, bool newState)
    {
        ApplyVisualState(newState);
    }

    private void ApplyVisualState(bool isActive)
    {
        sprite.enabled = isActive;
        collision.enabled = isActive;
    }
}