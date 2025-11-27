using Sources.Features.Popup.Model;

namespace Sources.Features.GlobeScreen.Model
{
    public class GlobePointData
    {
        public string Id { get; set; }
        public int GroupIndex { get; set; } 
        
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        
        public string Date { get; set; }
        public string Place { get; set; }
        public string Region { get; set; }
        public string Text { get; set; }
        public string MediaPath { get; set; }

        public PopupData ToPopupData()
        {
            return new PopupData
            {
                Date = Date,
                Place = Place,
                Region = Region,
                Text = Text,
                MediaPath = MediaPath,
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }
}