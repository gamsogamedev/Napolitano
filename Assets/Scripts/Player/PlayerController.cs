using Player.States;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        private enum PlayerStateType
        {
            Cone,
            IceCream,
            Spoon
        }

        [Header("Configurações de Movimento")] 
        [SerializeField] private float coneSpeed = 4f;
        [SerializeField] private float iceCreamSpeed = 6f;
        [SerializeField] private float spoonFollowSpeed = 15f;
        [SerializeField] private float jumpForce = 15f;

        [Header("Configurações de Colisão")] 
        [SerializeField] private Collider2D coneCollider;
        [SerializeField] private Collider2D iceCreamCollider;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Header("Input")] 
        [SerializeField] private InputActionAsset inputActions;

        [Header("Referências")] 
        [SerializeField] private Interact interactComponent;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private Transform spoonHoldPoint;
        
        [Header("Visuais por Estado")] 
        [SerializeField] private Color coneColor = Color.yellow;
        [SerializeField] private Color iceCreamColor = new Color(1f, 0.5f, 0.8f);
        [SerializeField] private Vector3 coneSpriteScale = new Vector3(1f, 2f, 1f);
        [SerializeField] private Vector3 iceCreamSpriteScale = new Vector3(1f, 1f, 1f);
        [SerializeField] private float coneGroundCheckY = -1;
        [SerializeField] private float iceCreamGroundCheckY = -0.5f;
        
        private InputAction _interactAction;
        private InputAction _jumpAction;
        private InputAction _moveAction;

        private readonly NetworkVariable<PlayerStateType> _networkedState = new(
            PlayerStateType.Cone,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public ConeState ConeState { get; private set; }
        public IceCreamState IceCreamState { get; private set; }
        public SpoonState SpoonState { get; private set; }

        public Rigidbody2D Rb { get; private set; }
        public Collider2D ConeCollider => coneCollider;
        public Collider2D IceCreamCollider => iceCreamCollider;
        public Interact InteractComponent => interactComponent;
        public Transform SpoonHoldPoint => spoonHoldPoint;
        public Vector2 MoveInput { get; private set; }
        public float JumpForce => jumpForce;
        public IPlayerState CurrentState { get; private set; }

        public bool JumpInputThisFrame => _jumpAction?.WasPressedThisFrame() ?? false;
        public bool InteractInputThisFrame => _interactAction?.WasPressedThisFrame() ?? false;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();

            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
            interactComponent ??= GetComponent<Interact>();

            ConeState = new ConeState(coneSpeed);
            IceCreamState = new IceCreamState(iceCreamSpeed);
            SpoonState = new SpoonState(spoonFollowSpeed);
        }

        private void Update()
        {
            if (!IsOwner) return;

            HandleInput();
            
            CurrentState?.Execute(this);
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            CurrentState?.HandleMovement(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!groundCheckPoint) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _networkedState.OnValueChanged += OnNetworkedStateChanged;

            if (!IsOwner)
            {
                ApplyStateVisuals(_networkedState.Value);
                return;
            }

            if (inputActions != null)
            {
                var playerMap = inputActions.FindActionMap("Player", true);
                _moveAction = playerMap.FindAction("Move", true);
                _interactAction = playerMap.FindAction("Interact", true);
                _jumpAction = playerMap.FindAction("Jump", true);

                playerMap.Enable();
            }
            else
            {
                Debug.LogError("[PlayerController] InputActionAsset não atribuído no Inspector!");
            }

            ChangeState(ConeState);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _networkedState.OnValueChanged -= OnNetworkedStateChanged;
            
            inputActions?.FindActionMap("Player")?.Disable();
        }

        public void ChangeState(IPlayerState newState)
        {
            if (newState == null) return;

            CurrentState?.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);

            var stateType = GetStateType(newState);
            ApplyStateVisuals(stateType);

            if (IsOwner)
                _networkedState.Value = GetStateType(newState);
        }

        private void HandleInput()
        {
            MoveInput = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public void SetFacingDirection(float directionX)
        {
            if (!spriteRenderer) return;
            if (directionX != 0) spriteRenderer.flipX = directionX < 0;
        }

        public bool IsGrounded()
        {
            if (!groundCheckPoint) return false;
            return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }

        private PlayerStateType GetStateType(IPlayerState state)
        {
            return state == ConeState ? PlayerStateType.Cone :
                state == IceCreamState ? PlayerStateType.IceCream :
                state == SpoonState ? PlayerStateType.Spoon :
                PlayerStateType.Cone;
        }

        private void OnNetworkedStateChanged(PlayerStateType oldState, PlayerStateType newState)
        {
            if (!IsOwner) ApplyStateVisuals(newState);
        }

        private void ApplyStateVisuals(PlayerStateType stateType)
        {
            if (spriteRenderer)
            {
                spriteRenderer.color = stateType switch
                {
                    PlayerStateType.Cone => coneColor,
                    PlayerStateType.IceCream => iceCreamColor,
                    _ => Color.white
                };
            }

            if (spriteRenderer)
            {
                spriteTransform.localScale = stateType switch
                {
                    PlayerStateType.Cone => coneSpriteScale,
                    PlayerStateType.IceCream => iceCreamSpriteScale,
                    _ => Vector3.one
                };
            }

            if (groundCheckPoint)
            {
                groundCheckPoint.localPosition = new Vector3(0f, stateType switch
                {
                    PlayerStateType.Cone => coneGroundCheckY,
                    PlayerStateType.IceCream => iceCreamGroundCheckY,
                    _ => -0.5f
                }, 0f);
            }
        }
    }
}