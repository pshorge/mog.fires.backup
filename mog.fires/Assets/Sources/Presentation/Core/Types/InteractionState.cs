namespace Sources.Presentation.Core.Types
{
    public enum InteractionState
    {
        Roaming,        // Moving (earth/map) and searching candidates (points), Timeline active
        Disambiguation, // Freeze moving, input controls menu list, Timeline frozen
        Details         // Show popup details, Timeline frozen
    }
}