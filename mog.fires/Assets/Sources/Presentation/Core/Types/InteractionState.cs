namespace Sources.Presentation.Core.Types
{
    
    public enum InteractionState
    {
        Roaming,        // Moving and searching candidates (points)
        Disambiguation, // Freeze moving, mouse Y-Pos selects menu element
        Details         // show selected element
    }
}