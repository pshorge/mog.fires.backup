using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Features.ControlButtons.Model;
using Unity.Properties;

namespace Sources.Features.ControlButtons.Presenter
{
    /// <summary>
    /// Presenter for Control Panel
    /// Manages language switching and accessibility controls
    /// </summary>
    public class ControlPanelPresenter : BaseDataSource, IPresenter
    {
        private readonly ILocalizationService _localizationService;
        private ControlButtonsData _data;

        private string _nextLanguageLabel;
        [CreateProperty]
        public string NextLanguageLabel
        {
            get => _nextLanguageLabel;
            private set { _nextLanguageLabel = value; Notify(); }
        }

        public ControlPanelPresenter(ILocalizationService localizationService)
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
            _data = new ControlButtonsData
            {
                NextLanguageLabel = _localizationService.GetNextLanguageTag().ToUpper()
            };
        }

        private void UpdateBindableProperties()
        {
            NextLanguageLabel = _data.NextLanguageLabel;
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