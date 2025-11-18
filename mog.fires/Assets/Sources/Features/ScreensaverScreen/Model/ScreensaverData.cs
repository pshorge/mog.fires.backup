namespace Sources.Features.ScreensaverScreen.Model
{
    /// <summary>
    /// Pure data model for Screensaver screen
    /// </summary>
    public class ScreensaverData
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string BackgroundFilePath { get; set; }
        public bool HasVideoBg { get; set; }
    }
}