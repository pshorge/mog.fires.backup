namespace Sources.Features.RightPopup.Model
{
    /// <summary>
    /// Pure data model for Right Popup
    /// </summary>
    public class RightPopupData
    {
        public string Date { get; set; }
        public string Place { get; set; }
        public string Region { get; set; }
        public string Text { get; set; }
        public string Stat1 { get; set; }
        public string Stat2 { get; set; }
        public string Stat3 { get; set; }
        public string Stat4 { get; set; }
        
        public float Latitude { get; set; }   // -90 to 90
        public float Longitude { get; set; }  // -180 to 180
    }
}