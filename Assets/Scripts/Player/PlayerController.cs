using Player.States;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;
using TMPro;
using Unity.Services.Multiplayer;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        public enum PlayerStateType
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
        [SerializeField] private GameObject spoonPrefab;
        [SerializeField] private NetworkObject conePrefab;
        [SerializeField] private SpriteRenderer coneSpriteRenderer;
        
        public Spoon CarriedSpoon { get; private set; }
        
        [Header("Visuais por Estado")] 
        [SerializeField] private Color iceCreamColor = new Color(1f, 0.5f, 0.8f);
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

        private TMP_Text playerNameText;

        private readonly NetworkVariable<FixedString64Bytes> _playerName = new(
            "",
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
        public PlayerStateType NetworkedStateType => _networkedState.Value;

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

        private void CreatePlayerNameText() //cria para ambos os jogadores o objeto nome TextMeshPro, nome visível no mundo
        {
            GameObject textObjt = new GameObject("PlayerName");

            textObjt.transform.SetParent(transform, false);
            textObjt.transform.localPosition = new Vector3(0f, 2f, 0f);

            playerNameText = textObjt.AddComponent<TextMeshPro>();

            playerNameText.alignment = TextAlignmentOptions.Center;
            playerNameText.fontSize = 3;
        }

        private void UpdatePlayerNameText(string playerName) //atualiza UI
        {
            if (playerNameText != null){
                playerNameText.text = playerName;
            }
        }

        private void InitializePlayerName() //inicializa nome do dono
        {
            if (!IsOwner) return;

            string playerName = SessionManager.Instance.GetLocalPlayerName();
            
            _playerName.Value = playerName;
            
            UpdatePlayerNameText(playerName);

        }

        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CreatePlayerNameText();

            _networkedState.OnValueChanged += OnNetworkedStateChanged;
            _playerName.OnValueChanged += OnPlayerNameChanged;

            InitializePlayerName();
            UpdatePlayerNameText(_playerName.Value.ToString());


            if (!IsOwner)
            {
                ApplyStateConfigutarion(_networkedState.Value);
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

            if (spoonPrefab != null)
            {
                var spoonObj = Instantiate(spoonPrefab, spoonHoldPoint.position, Quaternion.identity);
                var spoonNet = spoonObj.GetComponent<NetworkObject>();
                spoonNet.SpawnWithOwnership(OwnerClientId, true);

                CarriedSpoon = spoonObj.GetComponent<Spoon>();
                CarriedSpoon.AttachTo(this);
            }
            
            ChangeState(ConeState);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _networkedState.OnValueChanged -= OnNetworkedStateChanged;
            _playerName.OnValueChanged -= OnPlayerNameChanged;
            
            inputActions?.FindActionMap("Player")?.Disable();
        }

        public void ChangeState(IPlayerState newState)
        {
            if (newState == null) return;

            if (CurrentState == ConeState && newState == IceCreamState && IsOwner && conePrefab)
            {
                var spawnPos = groundCheckPoint.position + new Vector3(0f, 0.75f, 0f);
                var coneNet = Instantiate(conePrefab, spawnPos, Quaternion.identity);
                coneNet.Spawn(true);
            }

            if (newState == IceCreamState && CarriedSpoon)
            {
                CarriedSpoon.Drop();
                CarriedSpoon = null;
            }
            
            CurrentState?.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);

            var stateType = GetStateType(newState);
            ApplyStateConfigutarion(stateType);

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

        public void SetCarriedSpoon(Spoon spoon) => CarriedSpoon = spoon;
        
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
            if (!IsOwner) ApplyStateConfigutarion(newState);
        }

        private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue) {
            UpdatePlayerNameText(newValue.ToString());
        }

        private void ApplyStateConfigutarion(PlayerStateType stateType)
        {
            if (spriteRenderer)
                spriteRenderer.color = stateType == PlayerStateType.Spoon ? Color.white : iceCreamColor;
            
            if (coneSpriteRenderer)
                coneSpriteRenderer.enabled = stateType == PlayerStateType.Cone;

            if (spriteTransform)
            {
                spriteTransform.localScale = stateType == PlayerStateType.Spoon ? Vector3.zero : iceCreamSpriteScale;
                spriteTransform.localPosition = new Vector3(0f, stateType == PlayerStateType.Cone ? 1f : 0f, 0f);
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
            
            if (coneCollider) coneCollider.enabled = stateType == PlayerStateType.Cone;
            if (iceCreamCollider) iceCreamCollider.enabled = stateType == PlayerStateType.IceCream;
        }
    }
}