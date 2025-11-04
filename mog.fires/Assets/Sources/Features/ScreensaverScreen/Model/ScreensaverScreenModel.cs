using Artigio.MVVMToolkit.Core.Infrastructure.FileSystem;
using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Sources.Data.Models;
using Unity.Properties;

namespace Sources.Features.ScreensaverScreen.Model
{
    public class ScreensaverScreenModel :  BindableObject, IViewModelDataSource
    {
      
        
        private readonly AppSettings _settings;
        private readonly ILocalizationService _localizationService;

        private static class ContentKeys
        {
            public const string TitleKey = "screensaver-screen-title";
            public const string TextKey = "screensaver-screen-text";
        }
        
        private string _title;
        [CreateProperty]
        public string Title
        {
            private get => _title;
            set { _title = value; Notify();}
        }
        private string _text;
        [CreateProperty]
        public string Text
        {
            get => _text;
            private set { _text = value; Notify();} 
        }
        
        private string _backgroundFilePath;
        [CreateProperty]
        public string BackgroundFilePath
        {
            get => _backgroundFilePath;
            private set { _backgroundFilePath = value; Notify(); } 
        }
        
        private bool _hasVideoBg;
        [CreateProperty]
        public bool HasVideoBg
        {
            get => _hasVideoBg;
            private set { _hasVideoBg = value; Notify(); } 
        }
        
        public ScreensaverScreenModel(AppSettings settings, ILocalizationService localizationService)
        {
            _settings = settings;
            _localizationService = localizationService;
            FetchModel();
        }
        
        public void FetchModel()
        {
            Title = _localizationService.GetTranslation(ContentKeys.TitleKey);
            Text = _localizationService.GetTranslation(ContentKeys.TextKey);

            var newBgPath =  ContentPathResolver.ResolveContentPath(_settings.ScreensaverFile);
            if (BackgroundFilePath != newBgPath)
                BackgroundFilePath = newBgPath;
            
            HasVideoBg = !string.IsNullOrEmpty(BackgroundFilePath) && BackgroundFilePath.EndsWith(".webm");
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