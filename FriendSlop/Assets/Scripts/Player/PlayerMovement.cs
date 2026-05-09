using PurrNet;
using UnityEngine;

public class PlayerMovement : NetworkIdentity {
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void OnSpawned() {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void Update() {
        // Movimento horizontal
        float move = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // Pulo
        if (Input.GetKeyDown(KeyCode.W)) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}