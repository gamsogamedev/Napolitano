using System.Linq;
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
            var player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
                .FirstOrDefault(p => p.OwnerClientId == interactorClientId && p.CurrentState == p.IceCreamState);

            if (!player) return;
            
            NotifyCollectedRpc(interactorClientId);
            NetworkObject.Despawn();
        }

        [Rpc(SendTo.Everyone)]
        private void NotifyCollectedRpc(ulong collectorClientId)
        {
            if (NetworkManager.Singleton.LocalClientId != collectorClientId) return;
            
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.OwnerClientId != collectorClientId) continue;
                player.ChangeState(player.ConeState);
                return;
            }
        }
    }
}
