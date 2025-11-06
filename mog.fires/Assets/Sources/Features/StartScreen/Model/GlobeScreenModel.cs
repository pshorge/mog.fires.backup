using Artigio.MVVMToolkit.Core.Infrastructure.FileSystem;
using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Unity.Properties;
using UnityEngine;

namespace Sources.Features.StartScreen.Model
{
    public class GlobeScreenModel :  BindableObject, IViewModelDataSource
    {
      
        
        private readonly ILocalizationService _localizationService;

        private static class ContentKeys
        {
            public const string BgKey = "globe-screen-bg";
            public const string ImageKey = "globe-screen-image";
            public const string TitleKey = "globe-screen-title";
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
        
        public GlobeScreenModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += FetchModel;
            FetchModel();
        }

        public void FetchModel()
        {
            Title = _localizationService.GetTranslation(ContentKeys.TitleKey);
            
            var newBgPath =  ContentPathResolver.ResolveContentPath(_localizationService.GetTranslation(ContentKeys.BgKey));
            if (BackgroundFilePath != newBgPath)
                BackgroundFilePath = newBgPath;
            
            var imagePath =  ContentPathResolver.ResolveContentPath(_localizationService.GetTranslation(ContentKeys.ImageKey));
            if (ImageFilePath != imagePath)
                ImageFilePath = imagePath;
            
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localizationService.LanguageChanged -= FetchModel;
            }
            base.Dispose(disposing);
        }
    }
}