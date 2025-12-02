using Sources.Features.Popup.Model;

namespace Sources.Data.Models
{
    /// <summary>
    /// Unified data model for points on both Globe and Map.
    /// </summary>
    public class PointData
    {
        // Logic identifiers
        public string Id { get; set; }
        public int GroupIndex { get; set; }

        // Coordinates
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        // Content
        public string Date { get; set; }
        public string Place { get; set; }
        public string Region { get; set; }
        public string Text { get; set; }
        public string MediaPath { get; set; }

        // Helper to create Popup Data
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