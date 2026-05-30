using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerController))]
    public class Interact : MonoBehaviour
    {
        [Header("Configurações de Detecção")]
        [SerializeField] private float detectionRadius = 1.5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform detectionOrigin;

        private IInteractable _currentInteractable;
        private PlayerController _playerController;
        private Rigidbody2D _playerRb;

        private readonly List<Collider2D> _hitResults = new();
        private ContactFilter2D _contactFilter;

        private void Awake()
        {
            if (detectionOrigin == null)
                detectionOrigin = transform;

            _playerController = GetComponent<PlayerController>();
            _playerRb = GetComponentInParent<Rigidbody2D>();

            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(interactableLayer);
            _contactFilter.useLayerMask = true;
            _contactFilter.useTriggers = false; 
        }

        public void CheckCollision()
        {
            _currentInteractable = null;

            var hitCount = Physics2D.OverlapCircle(
                detectionOrigin.position,
                detectionRadius,
                _contactFilter,
                _hitResults
            );

            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                if (_playerRb && _hitResults[i].attachedRigidbody == _playerRb) continue;
                
                if (!_hitResults[i].TryGetComponent<IInteractable>(out var interactable)) continue;

                var closestPoint = _hitResults[i].ClosestPoint(detectionOrigin.position);
                var offset = closestPoint - (Vector2)detectionOrigin.position;
                
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq >= closestDistance) continue;

                closestDistance = distanceSq;
                _currentInteractable = interactable;
            }
        }

        private void DoInteract()
        {
            if (_currentInteractable != null && !_currentInteractable.Equals(null) && _playerController)
            {
                _currentInteractable.Interact(_playerController);
            }
        }

        public void CheckAndInteract()
        {
            CheckCollision();
            DoInteract();
        }

        private void OnDrawGizmosSelected()
        {
            var origin = detectionOrigin ? detectionOrigin : transform;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin.position, detectionRadius);
        }
    }
}