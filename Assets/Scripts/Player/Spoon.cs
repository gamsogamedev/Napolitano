using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class Spoon : NetworkBehaviour, IInteractable
    {
        [SerializeField] private float followSpeed = 20f;

        private readonly NetworkVariable<bool> _isCarried = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private Transform _holdPoint;
        private Rigidbody2D _rb;

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
            _isCarried.Value = true;
            player.SetCarriedSpoon(this);
        }

        public void Drop()
        {
            if (!IsOwnedByServer) return;

            _holdPoint = null;
            _isCarried.Value = false;
        }

        private void FixedUpdate()
        {
            if (!IsOwner || !_isCarried.Value || !_holdPoint) return;

            _rb.MovePosition(Vector2.Lerp(_rb.position, _holdPoint.position, followSpeed * Time.fixedDeltaTime));
        }

        public void Interact(PlayerController interactor)
        {
            if (!IsOwner) return;

            if (_isCarried.Value)
            {
                OnSpoonInteractedRpc(interactor.OwnerClientId);
                return;
            }

            if (interactor.CurrentState != interactor.ConeState) return;

            AttachTo(interactor);
        }
        
        private void OnCarriedChanged(bool previous, bool current)
        {
            if (_rb) _rb.bodyType = current ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        }

        [Rpc(SendTo.Everyone)]
        private void OnSpoonInteractedRpc(ulong interactorClientId)
        {
            Debug.Log("[Spoon] Jogador " + interactorClientId + " interagiu com a colher");
        }
    }
}