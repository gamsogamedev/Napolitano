using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class Cone : NetworkBehaviour, IInteractable
    {
        public void Interact(PlayerController interactor)
        {
            if (interactor.CurrentState != interactor.IceCreamState) return;
            
            RequestPickupRpc(interactor.OwnerClientId);
        }

        [Rpc(SendTo.Owner)]
        private void RequestPickupRpc(ulong interactorClientId)
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.OwnerClientId == interactorClientId && player.CurrentState == player.IceCreamState)
                {
                    NotifyCollectedRpc(interactorClientId);
                    NetworkObject.Despawn();
                    return;
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void NotifyCollectedRpc(ulong collectorClient)
        {
            if (NetworkManager.Singleton.LocalClientId != collectorClient) return;
            
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.OwnerClientId == collectorClient)
                {
                    player.ChangeState(player.ConeState);
                    return;
                }
            }
        }
    }
}
