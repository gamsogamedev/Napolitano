using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollision : NetworkBehaviour
{
    public static event Action<string> OnPlayerTookDamage;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;
        
        if (collision.gameObject.CompareTag("Hazard"))
        {
            BroadcastDamageRpc(SessionManager.Instance.ActiveSession.CurrentPlayer.Properties["playerName"].Value);
        }
    }
    
    [Rpc(SendTo.Everyone)]
    private void BroadcastDamageRpc(string playerName)
    {
        OnPlayerTookDamage?.Invoke(playerName);
    }
}
