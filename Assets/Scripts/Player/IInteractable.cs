namespace Player
{
    public interface IInteractable
    {
        bool CanInteract(PlayerController interactor);
        void Interact(PlayerController interactor);
    }
}