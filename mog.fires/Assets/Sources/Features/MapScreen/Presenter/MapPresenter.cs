using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Features.MapScreen.Model;
using Unity.Properties;

namespace Sources.Features.MapScreen.Presenter
{
    public class MapPresenter : BaseDataSource, IPresenter
    {
        private readonly ILocalizationService _localizationService;
        private MapData _data;

        private static class ContentKeys
        {
            public const string BgKey = "map-screen-bg";
        }

        private string _backgroundFilePath;
        [CreateProperty]
        public string BackgroundFilePath
        {
            get => _backgroundFilePath;
            private set { _backgroundFilePath = value; Notify(); }
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
                BackgroundFilePath = ContentPathResolver.ResolveContentPath(
                    _localizationService.GetTranslation(ContentKeys.BgKey)),
            };
        }

        private void UpdateBindableProperties()
        {
            BackgroundFilePath = _data.BackgroundFilePath;
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