using System.Collections.Generic;
using System.Linq;
using Artigio.MVVMToolkit.Core.Infrastructure.FileSystem;
using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Unity.Properties;
using UnityEngine;

namespace Sources.Features.GlobeScreen.Model
{
    public class GlobeScreenModel :  BindableObject, IViewModelDataSource
    {
      
        
        private readonly ILocalizationService _localizationService;

        private static class ContentKeys
        {
            public const string BgKey = "globe-screen-bg";
            public const string ImageKey = "globe-screen-image";
            public const string TitleKey = "globe-screen-title";
            
            public const string TimelineTitleKey = "mog-fires-timeline-title";
            public const string TimelineAllKey = "mog-fires-timeline-all";
        }
        
        private string _title;
        [CreateProperty]
        public string Title
        {
            get => _title;
            private set { _title = value; Notify();}
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
            private set { _timelineTitle = value; Notify();}
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
            set { if (_selectedStartIndex == value) return; _selectedStartIndex = value; Notify(); }
        }

        private int _selectedEndIndex = -1;
        [CreateProperty]
        public int SelectedEndIndex
        {
            get => _selectedEndIndex;
            set { if (_selectedEndIndex == value) return; _selectedEndIndex = value; Notify(); }
        }
        
        private bool _isTimelineSelectionFull;
        [CreateProperty] public bool IsTimelineSelectionFull
        {
            get => _isTimelineSelectionFull;
            set { if (_isTimelineSelectionFull == value) return; _isTimelineSelectionFull = value; Notify(); }
        }

        
        public GlobeScreenModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += FetchModel;
            FetchModel();
        }

        public void FetchModel()
        {
            Title = _localizationService.GetTranslation(ContentKeys.TitleKey);
            TimelineTitle = _localizationService.GetTranslation(ContentKeys.TimelineTitleKey);
            
            var newBgPath =  ContentPathResolver.ResolveContentPath(_localizationService.GetTranslation(ContentKeys.BgKey));
            if (BackgroundFilePath != newBgPath)
                BackgroundFilePath = newBgPath;
            
            var imagePath =  ContentPathResolver.ResolveContentPath(_localizationService.GetTranslation(ContentKeys.ImageKey));
            if (ImageFilePath != imagePath)
                ImageFilePath = imagePath;


            TimelinePeriods = FetchGlobeTimelineNames().ToList();
            InitOrClampSelection();
            RecalcSelectionFull();

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localizationService.LanguageChanged -= FetchModel;
            }
            base.Dispose(disposing);
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
        
        private void InitOrClampSelection()
        {
            int n = TimelinePeriods?.Count ?? 0;
        
            if (n < 2)
            {
                SelectedStartIndex = -1;
                SelectedEndIndex   = -1;
                return;
            }
        
            if (SelectedStartIndex < 0 && SelectedEndIndex < 0)
            {
                SelectedStartIndex = 0;
                SelectedEndIndex   = 1;
                return;
            }
        
            int s = Mathf.Clamp(SelectedStartIndex, 0, Mathf.Max(0, n - 1));
            int e = Mathf.Clamp(SelectedEndIndex,   1, n);
            if (e <= s) e = Mathf.Min(n, s + 1);
        
            SelectedStartIndex = s;
            SelectedEndIndex   = e;
        }
        
        private void RecalcSelectionFull()
        {
            int n = TimelinePeriods?.Count ?? 0;
            if (n <= 0) { IsTimelineSelectionFull = false; return; }

            bool full = (SelectedStartIndex == n - 1 && SelectedEndIndex == n);
            if (IsTimelineSelectionFull != full)
                IsTimelineSelectionFull = full;
            
        }
        
    }
    
    
}