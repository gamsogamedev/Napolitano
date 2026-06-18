using UnityEngine;

namespace Player.States
{
    public class IceCreamState : IPlayerState
    {
        private readonly float _speed;

        private float timer;
        private bool hasMelted;
        public float time_to_melt = 5f;

        public IceCreamState(float speed)
        {
            _speed = speed;
        }

        public void EnterState(PlayerController player)
        {
            timer = 0f;
            hasMelted = false;

        }

        public void Execute(PlayerController player)
        {

            timer += Time.deltaTime;

            if (!hasMelted && timer >= time_to_melt) {
                hasMelted = true;

                PlayerCollision playerCollision = player.GetComponent<PlayerCollision>();

                if (playerCollision != null) {
                    playerCollision.IceCream_Melted();
                }

                return;
            }

            if (player.JumpInputThisFrame && player.IsGrounded())
            {
                var velocity = player.Rb.linearVelocity;
                velocity.y = player.JumpForce;
                player.Rb.linearVelocity = velocity;
            }

            if (player.InteractInputThisFrame && player.InteractComponent) player.InteractComponent.CheckAndInteract();
        }

        public void ExitState(PlayerController player)
        {
            timer = 0f;
            hasMelted = false;
        }

        public void HandleMovement(PlayerController player)
        {
            var input = player.MoveInput;
            var velocity = player.Rb.linearVelocity;
            velocity.x = input.x * _speed;
            player.Rb.linearVelocity = velocity;

            if (input.x != 0) player.SetFacingDirection(input.x);
        }
    }
}