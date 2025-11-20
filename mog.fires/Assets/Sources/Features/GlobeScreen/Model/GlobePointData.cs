using Sources.Features.RightPopup.Model;

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
        
        public string Stat1 { get; set; }
        public string Stat2 { get; set; }
        public string Stat3 { get; set; }
        public string Stat4 { get; set; }

        public RightPopupData ToPopupData()
        {
            return new RightPopupData
            {
                Date = Date,
                Place = Place,
                Region = Region,
                Text = Text,
                Stat1 = Stat1,
                Stat2 = Stat2,
                Stat3 = Stat3,
                Stat4 = Stat4,
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }
}