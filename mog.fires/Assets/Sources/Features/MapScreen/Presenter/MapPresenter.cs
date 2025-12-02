using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Data.Models;
using Sources.Features.MapScreen.Model;
using Unity.Properties;
using UnityEngine;

namespace Sources.Features.MapScreen.Presenter
{
    public class MapPresenter : BaseDataSource, IPresenter
    {
        private readonly ILocalizationService _localizationService;
        private MapData _data;
        private List<PointData> _allPoints = new();


        private static class ContentKeys
        {
            public const string BgKey = "map-screen-bg";
            public const string MapKey = "map-screen-map";
            public const string TitleKey = "map-screen-title";
            public const string TimelineTitleKey = "mog-fires-timeline-title";
            
            public const string TimelineAllKey = "mog-fires-timeline-all";
            
            public const string ListKey = "mog-fires-list-map";
            public const string GroupItemsKey = "mog-fires-items";
            public const string GroupNameKey = "mog-fires-group-name";
        }

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
        
        private string _mapFilePath;
        [CreateProperty]
        public string MapFilePath
        {
            get => _mapFilePath;
            private set { _mapFilePath = value; Notify(); }
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
        
        private List<PointData> _visiblePoints = new();
        [CreateProperty]
        public List<PointData> VisiblePoints
        {
            get => _visiblePoints;
            private set { _visiblePoints = value; Notify(); }
        }

        public MapPresenter(ILocalizationService localizationService)
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
            _data = new MapData
            {
                Title = _localizationService.GetTranslation(ContentKeys.TitleKey),
                TimelineTitle = _localizationService.GetTranslation(ContentKeys.TimelineTitleKey),
                BackgroundFilePath = ContentPathResolver.ResolveContentPath(
                    _localizationService.GetTranslation(ContentKeys.BgKey)),
                MapFilePath = ContentPathResolver.ResolveContentPath(
                    _localizationService.GetTranslation(ContentKeys.MapKey)),
                TimelinePeriods = FetchMapTimelineNames().ToList(),
                SelectedStartIndex = _selectedStartIndex,
                SelectedEndIndex = _selectedEndIndex
            };
            
            _allPoints = FetchAllMapPoints();

            InitOrClampSelection();
            FilterVisiblePoints();
        }

        private void UpdateBindableProperties()
        {
            Title = _data.Title;
            TimelineTitle = _data.TimelineTitle;
            BackgroundFilePath = _data.BackgroundFilePath;
            MapFilePath = _data.MapFilePath;
            TimelinePeriods = _data.TimelinePeriods;
            
            if (_data.SelectedStartIndex != _selectedStartIndex)
                SelectedStartIndex = _data.SelectedStartIndex;
            if (_data.SelectedEndIndex != _selectedEndIndex)
                SelectedEndIndex = _data.SelectedEndIndex;

        }
        
         private IEnumerable<string> FetchMapTimelineNames()
        {
            const string prefix = "mog-fires-list-map";
            const string namePostfix = "mog-fires-group-name";
            int index = 0;
            
            while (true)
            {
                ++index;
                if (! _localizationService.TryGetTranslation($"{prefix}-{index}-{namePostfix}", out var name))
                    break;
                yield return name;
            }
            yield return _localizationService.GetTranslation(MapPresenter.ContentKeys.TimelineAllKey);
        }
        
        private List<PointData> FetchAllMapPoints()
        {
            var points = new List<PointData>();
            int groupIndex = 0;

            // Loop through Groups
            while (true)
            {
                ++groupIndex;
                // Key: mog-fires-list-map-{groupIndex}-mog-fires-group-name
                // i.e. mog-fires-list-map-1-mog-fires-group-name
                var groupNameKey = $"{MapPresenter.ContentKeys.ListKey}-{groupIndex}-{MapPresenter.ContentKeys.GroupNameKey}";
                
                // Check if group exists
                if (!_localizationService.TryGetTranslation(groupNameKey, out _)) 
                    break;
                
                int itemIndex = 0;
                // Loop through Items within Group
                while (true)
                {
                    ++itemIndex;
                    
                    // Key: mog-fires-list-map-{groupIndex}-mog-fires-items-{itemIndex}
                    // i.e. mog-fires-list-map-1-mog-fires-items-1
                    var itemPrefix = $"{MapPresenter.ContentKeys.ListKey}-{groupIndex}-{MapPresenter.ContentKeys.GroupItemsKey}-{itemIndex}";
                    
                    // Check if item exists (using 'pos' property as check)
                    if (!_localizationService.TryGetTranslation($"{itemPrefix}-mog-fires-item-pos", out var posStr))
                        break;

                    if (!TryParseCoordinates(posStr, out var lat, out var lon))
                    {
                        Debug.LogWarning($"Could not parse coordinates for item pos: {posStr}");
                        break;
                    }
                    
                    
                    var point = new PointData
                    {
                        Id = itemPrefix,
                        GroupIndex = groupIndex - 1, // 0-based for timeline logic
                        Place = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-place"),
                        Date = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-date"),
                        Region = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-region"),
                        Text = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-text"),
                        MediaPath = ContentPathResolver.ResolveContentPath(
                            _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-media")),
                        
                        Latitude = lat,
                        Longitude = lon
                    };
                    points.Add(point);
                }
            }
            
            Debug.Log($"[MapPresenter] Fetched total points: {points.Count}");
            return points;
        }

        private static bool TryParseCoordinates(string rawData, out float lat, out float lon)
        {
            lat = 0f;
            lon = 0f;

            if (string.IsNullOrWhiteSpace(rawData)) 
                return false;

            var parts = rawData.Split(',');

            if (parts.Length != 2) 
                return false;
            
            var latSuccess = float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
            var lonSuccess = float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lon);

            return latSuccess && lonSuccess;
        }
        

        
        private void FilterVisiblePoints()
        {
            if (_allPoints == null || TimelinePeriods == null || TimelinePeriods.Count == 0)
            {
                VisiblePoints = new List<PointData>();
                return;
            }

            // "All"
            bool isLastElement = _selectedStartIndex >= TimelinePeriods.Count - 1;

            if (isLastElement)
                VisiblePoints = new List<PointData>(_allPoints);
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
                if(_localizationService is not null)
                    _localizationService.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }
    }
}