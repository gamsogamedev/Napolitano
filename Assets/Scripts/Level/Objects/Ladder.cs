using Player;
using UnityEngine;

public class Ladder : MonoBehaviour {
    [SerializeField] private float climbSpeed = 4f;

    private PlayerController _player;
    private float _originalGravity;

    private void OnTriggerEnter2D(Collider2D other) {

        if (other.TryGetComponent(out PlayerController player) && player.IsOwner)
        {
            _player = player;
            _originalGravity = player.Rb.gravityScale;

            player.Rb.gravityScale = 0f;
            player.Rb.linearVelocity = Vector2.zero;
        }
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
        if (other.TryGetComponent(out PlayerController player) && player == _player)
        {
            player.Rb.gravityScale = _originalGravity;

            _player = null;    
        }
    }
}