using System.Collections.Generic;
using UnityEngine;

public class VictoryZone : MonoBehaviour {
    [SerializeField] private int playersNeeded = 2;

    private HashSet<GameObject> playersInside =
        new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            playersInside.Add(other.gameObject);

            CheckVictory();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            playersInside.Remove(other.gameObject);
        }
    }

    private void CheckVictory() {
        if (playersInside.Count >= playersNeeded) {
            Debug.Log("VITÓRIA!");
        }
    }
}