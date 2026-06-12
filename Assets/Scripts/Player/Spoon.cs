using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Spoon : NetworkBehaviour, IInteractable
    {
        [SerializeField] private float rotationSmoothTime = 0.4f;
        [SerializeField] private SpriteRenderer spoonHeadSprite;
        
        [SerializeField] private Sprite headClean;
        [SerializeField] private Sprite headStrawberry;
        [SerializeField] private Sprite headVanilla;
        
        private float _rotationVelocity;
        
        private readonly NetworkVariable<bool> _isCarried = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private Transform _holdPoint;
        private Rigidbody2D _rb;
        private Vector2 _positionVelocity;
        private float _previousAngle;
        private float _rotationalSpeed;

        [SerializeField] private Transform riderHoldPoint;
        [SerializeField] public float baseLaunchSpeed = 1f;
        [SerializeField] public float launchMultiplier = 1f;

        private readonly NetworkVariable<ulong> _riderClientId = new(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        private readonly NetworkVariable<float> _networkedTargetAngle = new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isCarried.OnValueChanged += OnCarriedChanged;
            _riderClientId.OnValueChanged += OnRiderChanged;
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _isCarried.OnValueChanged -= OnCarriedChanged;
            _riderClientId.OnValueChanged -= OnRiderChanged;
        }

        public void AttachTo(PlayerController player)
        {
            _rotationVelocity = -0f;
            _isCarried.Value = true;
            player.SetCarriedSpoon(this);
        }

        public void Drop()
        {
            if (!IsOwner) return;

            if (_riderClientId.Value != ulong.MaxValue)
            {
                var riderId = _riderClientId.Value;
                _riderClientId.Value = ulong.MaxValue;
                NotifyExitSpoonRpc(riderId, Vector2.zero);
            }

            _holdPoint = null;
            _isCarried.Value = false;
        }

        private void EjectRider()
        {
            var riderId = _riderClientId.Value;
            var toRider = (Vector2)riderHoldPoint.position - (Vector2)transform.position;
            var radius = toRider.magnitude;
            var radialDir = radius > 0f ? toRider / radius : (Vector2)transform.up;
            var tangentialDir = new Vector2(-radialDir.y, radialDir.x) * Mathf.Sign(_rotationalSpeed);
            var omegaRad = _rotationalSpeed * Mathf.Deg2Rad;
            var tangentialSpeed = Mathf.Abs(omegaRad) * radius * launchMultiplier;
            var launchVelocity = tangentialSpeed >= baseLaunchSpeed
                ? tangentialDir * tangentialSpeed
                : radialDir * baseLaunchSpeed;
            _riderClientId.Value = ulong.MaxValue;
            NotifyExitSpoonRpc(riderId, launchVelocity);
        }
        
        private void UpdateSpoonSprite(ulong currentRiderId)
        {
            if (currentRiderId == ulong.MaxValue)
            {
                spoonHeadSprite.sprite = headClean;
                return;
            }
            
            if (PlayerController.AllPlayers.TryGetValue(currentRiderId, out PlayerController player))
            {
                switch (player.PlayerSprite)
                {
                    case PlayerSprite.Strawberry: spoonHeadSprite.sprite = headStrawberry; break;
                    case PlayerSprite.Vanilla:    spoonHeadSprite.sprite = headVanilla; break;
                    default:                      spoonHeadSprite.sprite = headStrawberry; break;
                }
            }

        }
        
        private void FixedUpdate()
        {
            if (!_isCarried.Value || !_holdPoint) return;
    
            _rb.MovePosition(_holdPoint.position);

            if (IsOwner)
            {
                var mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var direction = (mouseWorld - _holdPoint.position).normalized;
        
                _networkedTargetAngle.Value = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            }

            var currentAngle = _rb.rotation;
            var newAngle = Mathf.SmoothDampAngle(currentAngle, _networkedTargetAngle.Value, ref _rotationVelocity, rotationSmoothTime);
            _rb.MoveRotation(newAngle);

            _rotationalSpeed = Mathf.DeltaAngle(_previousAngle, _rb.rotation) / Time.fixedDeltaTime;
            _previousAngle = _rb.rotation;

            if (Mathf.Abs(_rotationalSpeed) > 10f)
            {
                spoonHeadSprite.flipY = _rotationalSpeed < 0;
            }
        }

        public bool CanInteract(PlayerController interactor)
        {
            if (IsOwner && _isCarried.Value) return true;
            if (!_isCarried.Value) return true;
            return _isCarried.Value && _riderClientId.Value == ulong.MaxValue &&
                   interactor.CurrentState == interactor.IceCreamState;
        }

        public void Interact(PlayerController interactor)
        {
            if (_isCarried.Value && IsOwner)
            {
                Drop();
                return;
            }

            if (_isCarried.Value && _riderClientId.Value == ulong.MaxValue &&
                interactor.CurrentState == interactor.IceCreamState)
            {
                RequestEnterSpoonRpc(interactor.OwnerClientId);
                return;
            }

            RequestPickupRpc(interactor.OwnerClientId);
        }

        [Rpc(SendTo.Owner)]
        private void RequestEnterSpoonRpc(ulong riderClientId)
        {
            if (_riderClientId.Value != ulong.MaxValue) return;
            _riderClientId.Value = riderClientId;
            NotifyEnterSpoonRpc(riderClientId);
        }

        [Rpc(SendTo.Everyone)]
        private void NotifyEnterSpoonRpc(ulong riderClientId)
        {
            if (NetworkManager.Singleton.LocalClientId != riderClientId) return;
            if (PlayerController.AllPlayers.TryGetValue(riderClientId, out var localPlayer))
                localPlayer.EnterSpoonAsRider(riderHoldPoint, this);
        }

        [Rpc(SendTo.Owner)]
        public void RequestExitSpoonRpc(ulong riderClientId)
        {
            if (_riderClientId.Value != riderClientId) return;
            EjectRider();
        }

        [Rpc(SendTo.Everyone)]
        private void NotifyExitSpoonRpc(ulong riderClientId, Vector2 launchVelocity)
        {
            if (NetworkManager.Singleton.LocalClientId != riderClientId) return;
            if (PlayerController.AllPlayers.TryGetValue(riderClientId, out var localPlayer))
                localPlayer.ExitSpoonWithVelocity(launchVelocity);
        }

        [Rpc(SendTo.Owner)]
        private void RequestPickupRpc(ulong interactorClientId)
        {
            if (interactorClientId == OwnerClientId)
            {
                if (PlayerController.AllPlayers.TryGetValue(interactorClientId, out var player))
                {
                    if (player.CurrentState == player.ConeState) AttachTo(player);
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

            if (PlayerController.AllPlayers.TryGetValue(newOwnerClientId, out var player))
                if (player.CurrentState == player.ConeState)  AttachTo(player);
        }

        private void OnCarriedChanged(bool previous, bool current)
        {
            foreach (var col in GetComponents<Collider2D>())
                col.isTrigger = current;
        
            if (_rb) _rb.bodyType = current ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

            if (!current) return;
            _holdPoint = PlayerController.AllPlayers.TryGetValue(OwnerClientId, out var ownerPlayer) ? ownerPlayer.SpoonHoldPoint : null;
        }
        
        private void OnRiderChanged(ulong previousValue, ulong newValue)
        {
            UpdateSpoonSprite(newValue);
        }
    }
}