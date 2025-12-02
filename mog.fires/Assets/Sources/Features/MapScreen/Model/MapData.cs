using System.Collections.Generic;

namespace Sources.Features.MapScreen.Model
{
    public class MapData
    {
        public string Title { get; set; }
        public string BackgroundFilePath { get; set; }
        public string MapFilePath { get; set; }
        public string TimelineTitle { get; set; }
        public List<string> TimelinePeriods { get; set; } = new();
        public int SelectedStartIndex { get; set; } = -1;
        public int SelectedEndIndex { get; set; } = -1;
        public bool IsTimelineSelectionFull { get; set; }
    }
}