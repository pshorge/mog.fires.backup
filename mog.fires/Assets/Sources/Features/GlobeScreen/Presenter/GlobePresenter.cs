using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Features.GlobeScreen.Model;
using Unity.Properties;
using UnityEngine;

namespace Sources.Features.GlobeScreen.Presenter
{
    /// <summary>
    /// Presenter for Globe screen
    /// Manages timeline data and selection logic
    /// </summary>
    public class GlobePresenter : BaseDataSource, IPresenter
    {
        private readonly ILocalizationService _localizationService;
        private GlobeData _data;
        private List<GlobePointData> _allPoints = new();


        private static class ContentKeys
        {
            public const string BgKey = "globe-screen-bg";
            public const string ImageKey = "globe-screen-image";
            public const string TitleKey = "globe-screen-title";
            public const string TimelineTitleKey = "mog-fires-timeline-title";
            public const string TimelineAllKey = "mog-fires-timeline-all";
            
            public const string ListKey = "mog-fires-list-globe";
            public const string GroupItemsKey = "mog-fires-items";
            public const string GroupNameKey = "mog-fires-group-name";
        }

        // Bindable properties
        private string _title;
        [CreateProperty]
        public string Title
        {
            get => _title;
            private set { _title = value; Notify(); }
        }

        private string _backgroundFilePath;
        [CreateProperty]
        public string BackgroundFilePath
        {
            get => _backgroundFilePath;
            private set { _backgroundFilePath = value; Notify(); }
        }

        private string _imageFilePath;
        [CreateProperty]
        public string ImageFilePath
        {
            get => _imageFilePath;
            private set { _imageFilePath = value; Notify(); }
        }

        private string _timelineTitle;
        [CreateProperty]
        public string TimelineTitle
        {
            get => _timelineTitle;
            private set { _timelineTitle = value; Notify(); }
        }

        private IList<string> _timelinePeriods;
        [CreateProperty]
        public IList<string> TimelinePeriods
        {
            get => _timelinePeriods;
            private set { _timelinePeriods = value ?? new List<string>(); Notify(); }
        }

        private int _selectedStartIndex = -1;
        [CreateProperty]
        public int SelectedStartIndex
        {
            get => _selectedStartIndex;
            set 
            { 
                if (_selectedStartIndex == value) return; 
                _selectedStartIndex = value; 
                Notify(); 
                UpdateSelectionFullFlag();
                FilterVisiblePoints();
            }
        }

        private int _selectedEndIndex = -1;
        [CreateProperty]
        public int SelectedEndIndex
        {
            get => _selectedEndIndex;
            set 
            { 
                if (_selectedEndIndex == value) return; 
                _selectedEndIndex = value; 
                Notify(); 
                UpdateSelectionFullFlag();
            }
        }

        private bool _isTimelineSelectionFull;
        [CreateProperty]
        public bool IsTimelineSelectionFull
        {
            get => _isTimelineSelectionFull;
            private set { if (_isTimelineSelectionFull == value) return; _isTimelineSelectionFull = value; Notify(); }
        }
        
        private List<GlobePointData> _visiblePoints = new();
        [CreateProperty]
        public List<GlobePointData> VisiblePoints
        {
            get => _visiblePoints;
            private set { _visiblePoints = value; Notify(); }
        }

        public GlobePresenter(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += OnLanguageChanged;
            Initialize();
        }

        public void Initialize()
        {
            FetchData();
            UpdateBindableProperties();
        }

        public void RefreshData()
        {
            FetchData();
            UpdateBindableProperties();
        }

        private void FetchData()
        {
            _data = new GlobeData
            {
                Title = _localizationService.GetTranslation(ContentKeys.TitleKey),
                TimelineTitle = _localizationService.GetTranslation(ContentKeys.TimelineTitleKey),
                BackgroundFilePath = ContentPathResolver.ResolveContentPath(
                    _localizationService.GetTranslation(ContentKeys.BgKey)),
                ImageFilePath = ContentPathResolver.ResolveContentPath(
                    _localizationService.GetTranslation(ContentKeys.ImageKey)),
                TimelinePeriods = FetchGlobeTimelineNames().ToList(),
                SelectedStartIndex = _selectedStartIndex,
                SelectedEndIndex = _selectedEndIndex
            };
            
            _allPoints = FetchAllGlobePoints();

            InitOrClampSelection();
            FilterVisiblePoints();
        }

        private void UpdateBindableProperties()
        {
            Title = _data.Title;
            TimelineTitle = _data.TimelineTitle;
            BackgroundFilePath = _data.BackgroundFilePath;
            ImageFilePath = _data.ImageFilePath;
            TimelinePeriods = _data.TimelinePeriods;
            
            if (_data.SelectedStartIndex != _selectedStartIndex)
                SelectedStartIndex = _data.SelectedStartIndex;
            if (_data.SelectedEndIndex != _selectedEndIndex)
                SelectedEndIndex = _data.SelectedEndIndex;
        }

        private IEnumerable<string> FetchGlobeTimelineNames()
        {
            const string prefix = "mog-fires-list-globe";
            const string namePostfix = "mog-fires-group-name";
            int index = 0;
            
            while (true)
            {
                ++index;
                if (! _localizationService.TryGetTranslation($"{prefix}-{index}-{namePostfix}", out var name))
                    break;
                yield return name;
            }
            yield return _localizationService.GetTranslation(ContentKeys.TimelineAllKey);
        }
        
        private List<GlobePointData> FetchAllGlobePoints()
        {
            var points = new List<GlobePointData>();
            int groupIndex = 0;

            // Loop through Groups
            while (true)
            {
                ++groupIndex;
                // Key: mog-fires-list-globe-{groupIndex}-mog-fires-group-name
                // i.e. mog-fires-list-globe-1-mog-fires-group-name
                var groupNameKey = $"{ContentKeys.ListKey}-{groupIndex}-{ContentKeys.GroupNameKey}";
                
                // Check if group exists
                if (!_localizationService.TryGetTranslation(groupNameKey, out _)) 
                    break;
                
                int itemIndex = 0;
                // Loop through Items within Group
                while (true)
                {
                    ++itemIndex;
                    
                    // Key: mog-fires-list-globe-{groupIndex}-mog-fires-items-{itemIndex}
                    // i.e. mog-fires-list-globe-1-mog-fires-items-1
                    var itemPrefix = $"{ContentKeys.ListKey}-{groupIndex}-{ContentKeys.GroupItemsKey}-{itemIndex}";
                    
                    // Check if item exists (using 'place' property as check)
                    var placeKey = $"{itemPrefix}-mog-fires-item-place";
                    
                    if (!_localizationService.TryGetTranslation(placeKey, out var place))
                        break; // Break inner loop (items), proceed to next group



                    var tpoint = TestPointAtOrRandom(groupIndex-1);
                    
                    var point = new GlobePointData
                    {
                        Id = itemPrefix,
                        GroupIndex = groupIndex - 1, // 0-based for timeline logic
                        Place = place,
                        Date = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-date"),
                        Region = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-region"),
                        Text = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-text"),
                        
                        Stat1 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat1"),
                        Stat2 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat2"),
                        Stat3 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat3"),
                        Stat4 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat4"),
                        
                        
                        // Random Lat/Lon for now
                        Latitude = tpoint.lat,
                        Longitude = tpoint.lon
                    };

                    points.Add(point);
                }
            }
            
            Debug.Log($"[GlobePresenter] Fetched total points: {points.Count}");
            return points;
        }


        private (float lan, float lon)[] test_points = { (0f, 0f), (90f, 0f), (-30f, 120f), (50f, 20f), (41f, 17f) };
        (float lat, float lon) TestPointAtOrRandom(int index)
        {
            return index < test_points.Length  ? test_points[index] : (Random.Range(-90, 90f), Random.Range(-180f, 180f));;
        }
       

        
        private void FilterVisiblePoints()
        {
            if (_allPoints == null || TimelinePeriods == null || TimelinePeriods.Count == 0)
            {
                VisiblePoints = new List<GlobePointData>();
                return;
            }

            // "All"
            bool isLastElement = _selectedStartIndex >= TimelinePeriods.Count - 1;

            if (isLastElement)
                VisiblePoints = new List<GlobePointData>(_allPoints);
            else
                VisiblePoints = _allPoints.Where(p => p.GroupIndex == _selectedStartIndex).ToList();
        }

        private void InitOrClampSelection()
        {
            int n = _data.TimelinePeriods?.Count ?? 0;

            if (n < 2)
            {
                _data.SelectedStartIndex = -1;
                _data.SelectedEndIndex = -1;
                return;
            }

            if (_data.SelectedStartIndex < 0 && _data.SelectedEndIndex < 0)
            {
                _data.SelectedStartIndex = 0;
                _data.SelectedEndIndex = 1;
                return;
            }

            int s = Mathf.Clamp(_data.SelectedStartIndex, 0, Mathf.Max(0, n - 1));
            int e = Mathf.Clamp(_data.SelectedEndIndex, 1, n);
            if (e <= s) e = Mathf.Min(n, s + 1);

            _data.SelectedStartIndex = s;
            _data.SelectedEndIndex = e;
        }

        private void UpdateSelectionFullFlag()
        {
            int n = TimelinePeriods?.Count ?? 0;
            bool isFull = (n > 0 && _selectedStartIndex == n - 1 && _selectedEndIndex == n);
            IsTimelineSelectionFull = isFull;
        }

        private void OnLanguageChanged() => RefreshData();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localizationService.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }
    }
}