using Player;
using UnityEngine;

public class Ladder : MonoBehaviour {
    [SerializeField] private float climbSpeed = 4f;

    private PlayerController _player;
    private float _originalGravity;

    private void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;
        if (!player.IsOwner) return;

        // Só pode subir se estiver no cone
        if (player.CurrentState != player.ConeState) return;

        _player = player;
        _originalGravity = player.Rb.gravityScale;

        player.Rb.gravityScale = 0f;
        player.Rb.linearVelocity = Vector2.zero;

        Debug.Log("Player entrou na escada");
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!_player) return;

        Vector2 input = _player.MoveInput;

        Vector2 velocity = _player.Rb.linearVelocity;
        velocity.y = input.y * climbSpeed;
        velocity.x = input.x * 2f;

        _player.Rb.linearVelocity = velocity;
    }

    private void OnTriggerExit2D(Collider2D other) {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;
        if (player != _player) return;

        player.Rb.gravityScale = _originalGravity;

        _player = null;

        Debug.Log("Player saiu da escada");
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }
}