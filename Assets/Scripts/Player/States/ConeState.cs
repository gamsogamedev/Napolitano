namespace Player.States
{
    public class ConeState : IPlayerState
    {
        private readonly float _speed;

        public ConeState(float speed)
        {
            _speed = speed;
        }

        public void EnterState(PlayerController player)
        {
        }

        public void Execute(PlayerController player)
        {
            if (player.JumpInputThisFrame && player.IsGrounded()) 
            {
                player.ChangeState(player.IceCreamState);
            }

            if (player.InteractInputThisFrame)
            {
                if (player.CarriedSpoon)
                {
                    player.CarriedSpoon.Drop();
                    player.SetCarriedSpoon(null);
                }
                else if (player.InteractComponent)
                {
                    player.InteractComponent.CheckAndInteract();
                }
            }
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