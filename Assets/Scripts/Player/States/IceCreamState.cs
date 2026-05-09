namespace Player.States
{
    public class IceCreamState : IPlayerState
    {
        private readonly float _speed;

        public IceCreamState(float speed)
        {
            _speed = speed;
        }

        public void EnterState(PlayerController player)
        {
            if (player.IceCreamCollider) player.IceCreamCollider.enabled = true;
            if (player.ConeCollider) player.ConeCollider.enabled = false;
        }

        public void Execute(PlayerController player)
        {
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