using System.Collections.Generic;

namespace Sources.Features.GlobeScreen.Model
{
    /// <summary>
    /// Pure data model for Globe screen
    /// </summary>
    public class GlobeData
    {
        public string Title { get; set; }
        public string BackgroundFilePath { get; set; }
        public string TimelineTitle { get; set; }
        public List<string> TimelinePeriods { get; set; } = new();
        public int SelectedStartIndex { get; set; } = -1;
        public int SelectedEndIndex { get; set; } = -1;
        public bool IsTimelineSelectionFull { get; set; }
    }
}