namespace Sources.Features.Popup.Model
{
    /// <summary>
    /// Pure data model for Right Popup
    /// </summary>
    public class PopupData
    {
        public string Date { get; set; }
        public string Place { get; set; }
        public string Region { get; set; }
        public string Text { get; set; }
        public string MediaPath { get; set; }
        
        public float Latitude { get; set; } 
        public float Longitude { get; set; }
    }
}