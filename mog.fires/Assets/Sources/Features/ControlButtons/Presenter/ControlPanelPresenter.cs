using System;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Features.ControlButtons.Model;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
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
        private readonly IDisposable _changeLanguageInputSubscription;

        private ControlButtonsData _data;

        private string _nextLanguageLabel;

        [CreateProperty]
        public string NextLanguageLabel
        {
            get => _nextLanguageLabel;
            private set { _nextLanguageLabel = value; Notify(); }
        }

        public ControlPanelPresenter(ILocalizationService localizationService, IUnifiedInputService unifiedInput)
        {
            _localizationService = localizationService;
            _changeLanguageInputSubscription = unifiedInput.Subscribe(InputActionType.ChangeLanguage, _localizationService.ChangeLanguage);
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
                _changeLanguageInputSubscription.Dispose();
                if(_localizationService is not null)
                    _localizationService.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }
    }
}