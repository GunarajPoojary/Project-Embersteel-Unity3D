namespace ProjectEmbersteel.InteractionSystem
{
    public enum InteractionType { Chest, TrialSword };

    public interface IInteractable
    {
        bool Interactable { get; }
        InteractionType InteractionType { get; }
        void Interact();
    }
}