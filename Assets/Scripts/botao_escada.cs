using System.Collections.Generic;
using Player;
using UnityEngine;

public class PressurePlate : MonoBehaviour {
    [SerializeField] private GameObject ladderPrefab;
    [SerializeField] private Transform ladderSpawnPoint;

    private GameObject spawnedLadder;
    private readonly HashSet<PlayerController> playersOnPlate = new();

    private void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;

        playersOnPlate.Add(player);

        if (spawnedLadder == null) {
            spawnedLadder = Instantiate(
                ladderPrefab,
                ladderSpawnPoint.position,
                ladderSpawnPoint.rotation
            );
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;

        playersOnPlate.Remove(player);

        if (playersOnPlate.Count == 0 && spawnedLadder != null) {
            Destroy(spawnedLadder);
            spawnedLadder = null;
        }
    }
}