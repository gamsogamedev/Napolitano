namespace Player
{
    public interface IPlayerState
    {
        void EnterState(PlayerController player);
        void Execute(PlayerController player);
        void ExitState(PlayerController player);
        void HandleMovement(PlayerController player);
    }
}