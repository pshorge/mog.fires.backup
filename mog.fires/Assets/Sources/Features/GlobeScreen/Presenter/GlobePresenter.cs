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
            public const string GroupItemsKey = "mog-fires-group-items";
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
                var name = _localizationService.GetTranslation($"{prefix}-{index}-{namePostfix}");
                if (name is null) break;
                yield return name;
            }

            yield return _localizationService.GetTranslation(ContentKeys.TimelineAllKey);
        }
        
        private List<GlobePointData> FetchAllGlobePoints()
        {
            var points = new List<GlobePointData>();
            int groupIndex = 0;

            
            while (true)
            {
                ++groupIndex;
                // checks if group exists (by name)
                var groupNameKey = $"{ContentKeys.ListKey}-{groupIndex}-{ContentKeys.GroupNameKey}";
                if (!_localizationService.TryGetTranslation(groupNameKey, out _)) 
                    break;
                
                int itemIndex = 0;
                while (true)
                {
                    ++itemIndex;
                    // key: mog-fires-list-globe-{group}-{items}-{item}-date
                    var itemPrefix = $"{ContentKeys.ListKey}-{groupIndex}-{ContentKeys.GroupItemsKey}-{itemIndex}";
                    
                    // check if exists (by place)
                    var placeKey = $"{itemPrefix}-mog-fires-item-place";
                    if (!_localizationService.TryGetTranslation(placeKey, out var placeVal)) 
                        break;

                    
                    var point = new GlobePointData
                    {
                        Id = itemPrefix,
                        GroupIndex = groupIndex - 1,
                        Place = placeVal,
                        Date = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-date"),
                        Region = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-region"),
                        Text = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-text"),
                        
                        Stat1 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat-1"),
                        Stat2 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat-2"),
                        Stat3 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat-3"),
                        Stat4 = _localizationService.GetTranslation($"{itemPrefix}-mog-fires-item-stat-4"),
                        // parsing Lat/Lon (float invariant)
                        // if (_localizationService.TryGetTranslation($"{itemPrefix}-lat", out var latStr))
                        //     if (float.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
                        //         point.Latitude = lat;
                        //
                        // if (_localizationService.TryGetTranslation($"{itemPrefix}-lon", out var lonStr))
                        //     if (float.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
                        //         point.Longitude = lon;
                        Latitude = UnityEngine.Random.Range(-90f, 90f),
                        Longitude = UnityEngine.Random.Range(-180f, 180f)
                    };

                    // TODO: add lat,long to json  in artigio model
                    Debug.LogWarning("ADDING RANDOM POINTS!!");
                    points.Add(point);
                }
            }
            return points;
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