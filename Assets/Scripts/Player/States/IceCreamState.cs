using AudioSystem;
using UnityEngine;

namespace Player.States
{
    public class IceCreamState : IPlayerState
    {
        private readonly float _speed;
        private bool lastFrameGrounded;

        public IceCreamState(float speed)
        {
            _speed = speed;
        }

        public void EnterState(PlayerController player) {

            player.GetComponent<IceCreamMeltTimer>()?.StartTimer();
            lastFrameGrounded = true;
        }

        public void Execute(PlayerController player)
        {

            if (player.JumpInputThisFrame && player.IsGrounded())
            {
                var velocity = player.Rb.linearVelocity;
                velocity.y = player.JumpForce;
                player.Rb.linearVelocity = velocity;
                SoundManager.Instance.CreateSound().Play(player.jumpingSound);
                lastFrameGrounded = false;
            }

            if (!lastFrameGrounded && player.Rb.linearVelocity.y < 0 && player.IsGrounded())
            {
                SoundManager.Instance.CreateSound().Play(player.landingSound);
                lastFrameGrounded = true;
            }

            if (player.InteractInputThisFrame && player.InteractComponent) player.InteractComponent.CheckAndInteract();
        }

        public void ExitState(PlayerController player)
        {
            player.GetComponent<IceCreamMeltTimer>()?.StopTimer();
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