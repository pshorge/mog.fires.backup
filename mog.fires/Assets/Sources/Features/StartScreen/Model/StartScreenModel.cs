using Artigio.MVVMToolkit.Core.Infrastructure.FileSystem;
using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Unity.Properties;
using UnityEngine;

namespace Sources.Features.StartScreen.Model
{
    public class StartScreenModel :  BindableObject, IViewModelDataSource
    {
      
        
        private readonly ILocalizationService _localizationService;

        private static class ContentKeys
        {
            public const string BgKey = "start-screen-bg";
            public const string TitleKey = "start-screen-title";
        }
        
        private string _title;
        [CreateProperty]
        public string Title
        {
            get => _title;
            private set { _title = value; Notify();}
        }
        
        private string _backgroundImagePath;
        [CreateProperty]
        public string BackgroundImagePath
        {
            get => _backgroundImagePath; 
            private set { _backgroundImagePath = value; Notify(); }
        }
        
        
        public StartScreenModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += FetchModel;
            FetchModel();
        }

        public void FetchModel()
        {
            Title = _localizationService.GetTranslation(ContentKeys.TitleKey);
            
            var newBgPath =  ContentPathResolver.ResolveContentPath(_localizationService.GetTranslation(ContentKeys.BgKey));
            if (BackgroundImagePath != newBgPath)
                BackgroundImagePath = newBgPath;
            
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