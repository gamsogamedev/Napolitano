using UnityEngine;

namespace Player.States
{
    public class SpoonState : IPlayerState
    {
        private readonly float _followSpeed;
        private Transform _spoonPosition;
        private bool _isRider;
        private Spoon _spoon;

        public SpoonState(float followSpeed)
        {
            _followSpeed = followSpeed;
        }

        public void EnterState(PlayerController player)
        {
            if (!player.Rb) return;
            player.Rb.bodyType = RigidbodyType2D.Kinematic;
            player.Rb.linearVelocity = Vector2.zero;

            if (player.IceCreamCollider) player.IceCreamCollider.enabled = false;
            if (player.ConeCollider) player.ConeCollider.enabled = false;
        }

        public void Execute(PlayerController player)
        {
            if (!_spoonPosition)
            {
                player.ChangeState(player.IceCreamState);
                return;
            }

            if (!player.JumpInputThisFrame) return;

            if (_isRider && _spoon)
            {
                _spoon.RequestExitSpoonRpc(player.OwnerClientId);
            }
            else
            {
                player.ChangeState(player.IceCreamState);
                var velocity = player.Rb.linearVelocity;
                velocity.y = player.JumpForce;
                player.Rb.linearVelocity = velocity;
            }
        }

        public void ExitState(PlayerController player)
        {
            if (player.Rb) player.Rb.bodyType = RigidbodyType2D.Dynamic;
            _spoonPosition = null;
            _isRider = false;
            _spoon = null;
        }

        public void HandleMovement(PlayerController player)
        {
            if (!_spoonPosition) return;

            var nextPosition = Vector2.Lerp(
                player.Rb.position,
                _spoonPosition.position,
                _followSpeed * Time.fixedDeltaTime
            );

            player.Rb.MovePosition(nextPosition);
        }

        public void SetSpoonPosition(Transform position)
        {
            _spoonPosition = position;
        }

        public void SetRiderMode(Spoon spoon)
        {
            _isRider = true;
            _spoon = spoon;
        }
    }
}