using System;
using System.Collections.Generic;
using AudioSystem;
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
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private Transform spoonHoldPoint;
        [SerializeField] private GameObject spoonPrefab;
        [SerializeField] private NetworkObject conePrefab;
        [SerializeField] private SpriteRenderer coneSpriteRenderer;
        [SerializeField] private TextMeshPro playerNameField;
        
        public Spoon CarriedSpoon { get; private set; }
        
        [Header("Visuais por Estado")] 
        [SerializeField] private Color iceCreamColor = new Color(1f, 0.5f, 0.8f);
        [SerializeField] private float coneGroundCheckY = -1;
        [SerializeField] private float iceCreamGroundCheckY = -0.5f;
        
        [Header("Sprites")]
        [SerializeField] private Sprite strawberrySprite;
        [SerializeField] private Sprite vanillaSprite;
        
        [Header("Audio")] 
        [SerializeField] public SoundData popInSound;
        [SerializeField] public SoundData popOutSound;
        [SerializeField] public SoundData swooshSound;
        [SerializeField] public SoundData jumpingSound;
        [SerializeField] public SoundData landingSound;
        
        //You can call these audios witch SoundManager.Instance.CreateSound().Play(soundData)

        
        
        private InputAction _interactAction;
        private InputAction _jumpAction;
        private InputAction _moveAction;

        private readonly NetworkVariable<PlayerStateType> _networkedState = new(
            PlayerStateType.Cone,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        private readonly NetworkVariable<FixedString64Bytes> _playerName = new(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        private readonly NetworkVariable<PlayerSprite> _playerSprite = new(
            PlayerSprite.Strawberry,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        public static readonly Dictionary<ulong, PlayerController> AllPlayers = new Dictionary<ulong, PlayerController>();
        

        public ConeState ConeState { get; private set; }
        public IceCreamState IceCreamState { get; private set; }
        public SpoonState SpoonState { get; private set; }

        public Rigidbody2D Rb { get; private set; }
        public Collider2D ConeCollider => coneCollider;
        public Collider2D IceCreamCollider => iceCreamCollider;
        
        //Maybe you can use this, I just added it
        public PlayerSprite PlayerSprite => _playerSprite.Value;
        
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

            bodySpriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
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
        

        private void InitializePlayer()
        {
            if (!IsOwner) return;

            _playerName.Value = SessionManager.Instance.GetLocalPlayerName();
            _playerSprite.Value = SessionManager.Instance.GetLocalPlayerSprite();

        }

        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            AllPlayers[OwnerClientId] = this;
            
            _networkedState.OnValueChanged += OnNetworkedStateChanged;
            _playerName.OnValueChanged += OnPlayerNameChanged;
            _playerSprite.OnValueChanged += OnPlayerSpriteChanged;

            InitializePlayer();
            
            playerNameField.text = _playerName.Value.ToString();
            SetSprite(_playerSprite.Value);
            

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
            
            AllPlayers.Remove(OwnerClientId);

            _networkedState.OnValueChanged -= OnNetworkedStateChanged;
            _playerName.OnValueChanged -= OnPlayerNameChanged;
            _playerSprite.OnValueChanged -= OnPlayerSpriteChanged;
            
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
            if (directionX != 0) bodySpriteRenderer.flipX = directionX > 0;
        }

        public void SetCarriedSpoon(Spoon spoon) => CarriedSpoon = spoon;

        public void EnterSpoonAsRider(Transform spoonTransform, Spoon spoon)
        {
            SpoonState.SetSpoonPosition(spoonTransform);
            SpoonState.SetRiderMode(spoon);
            ChangeState(SpoonState);
        }

        public void ExitSpoonWithVelocity(Vector2 launchVelocity)
        {
            ChangeState(IceCreamState);
            Rb.linearVelocity = launchVelocity;
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
            if (!IsOwner) ApplyStateConfigutarion(newState);
        }

        private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue) {
            playerNameField.text = newValue.ToString();
        }
        
        private void OnPlayerSpriteChanged(PlayerSprite previousValue, PlayerSprite newValue)
        {
            switch (newValue)
            {
                case PlayerSprite.Strawberry: bodySpriteRenderer.sprite = strawberrySprite; break;
                case PlayerSprite.Vanilla:  bodySpriteRenderer.sprite = vanillaSprite; break;
                case PlayerSprite.Chocolate: Debug.LogWarning("PlayerController(288): acessando com skin de chocolate");break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        private void SetSprite(PlayerSprite sprite)
        {
            switch (sprite)
            {
                case PlayerSprite.Strawberry: bodySpriteRenderer.sprite = strawberrySprite; break;
                case PlayerSprite.Vanilla:  bodySpriteRenderer.sprite = vanillaSprite; break;
            }
        }

        private void ApplyStateConfigutarion(PlayerStateType stateType)
        {
            coneSpriteRenderer.enabled = stateType == PlayerStateType.Cone;
            
            SetSpoonVisibility(stateType != PlayerStateType.Spoon);

            groundCheckPoint.localPosition = new Vector3(0f, stateType switch
            {
                PlayerStateType.Cone => coneGroundCheckY,
                PlayerStateType.IceCream => iceCreamGroundCheckY,
                _ => -0.5f
            }, 0f);
            
            
            if (coneCollider) coneCollider.enabled = stateType == PlayerStateType.Cone;
            if (iceCreamCollider) iceCreamCollider.enabled = stateType == PlayerStateType.IceCream;
        }
        
        private void SetSpoonVisibility(bool visibility)
        {
            bodySpriteRenderer.enabled = visibility;
            playerNameField.enabled = visibility;
        }

        public void DisableController(bool hidePlayer) {
            GetComponent<PlayerActions>()?.DisablePlayer(hidePlayer);
            // Futuramente:
            // Animator.SetTrigger("Win");

        }
    }
}