using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Data.Models;
using Sources.Features.ScreensaverScreen.Model;
using Unity.Properties;

namespace Sources.Features.ScreensaverScreen.Presenter
{
    /// <summary>
    /// Presenter for Screensaver screen
    /// Handles business logic and data preparation
    /// </summary>
    public class ScreensaverPresenter : BaseDataSource, IPresenter
    {
        private readonly AppSettings _settings;
        private readonly ILocalizationService _localizationService;
        private ScreensaverData _data;

        private static class ContentKeys
        {
            public const string TitleKey = "screensaver-screen-title";
            public const string TextKey = "screensaver-screen-text";
        }

        // Bindable properties for UI
        private string _title;
        [CreateProperty]
        public string Title
        {
            get => _title;
            private set { _title = value; Notify(); }
        }

        private string _text;
        [CreateProperty]
        public string Text
        {
            get => _text;
            private set { _text = value; Notify(); }
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

        public ScreensaverPresenter(AppSettings settings, ILocalizationService localizationService)
        {
            _settings = settings;
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
            _data = new ScreensaverData
            {
                Title = _localizationService.GetTranslation(ContentKeys.TitleKey),
                Text = _localizationService.GetTranslation(ContentKeys.TextKey),
                BackgroundFilePath = ContentPathResolver.ResolveContentPath(_settings.ScreensaverFile)
            };
            
            _data.HasVideoBg = !string.IsNullOrEmpty(_data.BackgroundFilePath) 
                && _data.BackgroundFilePath.EndsWith(".webm");
        }

        private void UpdateBindableProperties()
        {
            Title = _data.Title;
            Text = _data.Text;
            BackgroundFilePath = _data.BackgroundFilePath;
            HasVideoBg = _data.HasVideoBg;
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