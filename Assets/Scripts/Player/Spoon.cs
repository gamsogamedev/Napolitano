using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Spoon : NetworkBehaviour, IInteractable
    {
        [SerializeField] private float rotationSmoothTime = 0.4f;
        private float _rotationVelocity;
        
        private readonly NetworkVariable<bool> _isCarried = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private Transform _holdPoint;
        private Rigidbody2D _rb;
        private Vector2 _positionVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isCarried.OnValueChanged += OnCarriedChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isCarried.OnValueChanged -= OnCarriedChanged;
        }

        public void AttachTo(PlayerController player)
        {
            _holdPoint = player.SpoonHoldPoint;
            _rotationVelocity = -0f;
            _isCarried.Value = true;
            player.SetCarriedSpoon(this);
        }

        public void Drop()
        {
            if (!IsOwner) return;

            _holdPoint = null;
            _isCarried.Value = false;
        }

        private void FixedUpdate()
        {
            if (!IsOwner || !_isCarried.Value || !_holdPoint) return;
            
            _rb.MovePosition(_holdPoint.position);

            var mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var direction = (mouseWorld - _holdPoint.position).normalized;
            var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            var currentAngle = _rb.rotation;
            var newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref _rotationVelocity, rotationSmoothTime);
            _rb.MoveRotation(newAngle);
        }
        
        public bool CanInteract(PlayerController interactor) => !_isCarried.Value == IsOwner;

        public void Interact(PlayerController interactor)
        {
            if (_isCarried.Value && IsOwner)
            {
                Drop();
                return;
            }

            RequestPickupRpc(interactor.OwnerClientId);
        }

        [Rpc(SendTo.Owner)]
        private void RequestPickupRpc(ulong interactorClientId)
        {
            if (interactorClientId == OwnerClientId)
            {
                var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    if (player.OwnerClientId == interactorClientId && player.CurrentState == player.ConeState)
                    {
                        AttachTo(player);
                        return;
                    }
                        
                }
            }
            else
            {
                NetworkObject.ChangeOwnership(interactorClientId);
                PickupAfterOwnershipRpc(interactorClientId);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void PickupAfterOwnershipRpc(ulong newOwnerClientId)
        {
            if (NetworkManager.Singleton.LocalClientId != newOwnerClientId) return;

            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.OwnerClientId == newOwnerClientId && player.CurrentState == player.ConeState)
                {
                    AttachTo(player);
                    return;
                }
            }
        }

        private void OnCarriedChanged(bool previous, bool current)
        {
            foreach (var col in GetComponents<Collider2D>())
                col.enabled = !current;
            if (_rb) _rb.bodyType = current ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        }
    }
}