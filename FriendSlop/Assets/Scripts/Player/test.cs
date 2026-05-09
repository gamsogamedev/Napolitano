using System;
using UnityEngine;
using PurrNet;
using TMPro;

public class test_ : NetworkIdentity {
    [SerializeField] private Color _color;
    [SerializeField] private SpriteRenderer _renderer;

    private LevelManager levelManager;

    [SerializeField] private SyncVar<int> _health = new(100, ownerAuth: true);

    private void Awake() {

        levelManager = FindFirstObjectByType<LevelManager>();

    }

    protected override void OnDestroy() {

        base.OnDestroy();

    }

    protected override void OnSpawned() {

        base.OnSpawned();

        enabled = isOwner;
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.C)) {
            SetColor(_color);
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            Take_Damage(10);
        }
    }

    [ObserversRpc(bufferLast: true)]
    private void SetColor(Color color) {

        _renderer.color = color;
    }

    private void Take_Damage(int damage) {

        _health.value -= damage;

        if (_health.value <= 0) {
            _health.value = 0;
            levelManager.PlayerDied();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Colidiu");

        if (collision.gameObject.CompareTag("Espinho")) {
            Debug.Log("Player colidiu");
            Take_Damage(20);
        }
    }

}
