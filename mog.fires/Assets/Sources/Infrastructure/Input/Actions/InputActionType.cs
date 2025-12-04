namespace Sources.Infrastructure.Input.Actions
{
    public enum InputActionType
    {
        None = 0,
        
        // Global
        ChangeLanguage,    // L
        QuitApp,           // ESC
        
        // View Navigation
        NavigateHome,      // R (Reset -> Screensaver)
        NavigateForward,   // Space
        NavigateBack,      // Backspace
        SwitchMode,        // M / MMB (Globe <-> Map)
        
        // Interactions (Timeline, Menu, Popup)
        Select,            // 1 / LMB
        Back,              // 2 / RMB
        
        //Scroll ? 
        /*
        NextItem,
        PreviousItem*/
    }
}