using Unity.Netcode;
using UnityEngine;

namespace Multiplayer
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        public override void OnNetworkSpawn()
        {
            if (!IsClient) return;
        
            var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        }
    }
}
