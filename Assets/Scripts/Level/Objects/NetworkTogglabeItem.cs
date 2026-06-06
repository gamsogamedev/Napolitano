using Unity.Netcode;
using UnityEngine;

public class NetworkTogglableItem : NetworkBehaviour
{
    [SerializeField] private Renderer sprite;
    [SerializeField] private Collider2D collision;
    
    private readonly NetworkVariable<bool> isItemActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
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