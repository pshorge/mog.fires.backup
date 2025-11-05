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
        
        private string _backgroundFilePath;
        [CreateProperty]
        public string BackgroundFilePath
        {
            get => _backgroundFilePath; 
            private set { _backgroundFilePath = value; Notify(); }
        }

        public void TestPath(string path)
        {
            BackgroundFilePath   = path;
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
            if (BackgroundFilePath != newBgPath)
                BackgroundFilePath = newBgPath;
            
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