using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public static event Action<string> OnPlayerTookDamage;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            OnPlayerTookDamage?.Invoke(SessionManager.Instance.ActiveSession.CurrentPlayer.Properties["playerName"].Value);
        }
    }
}
