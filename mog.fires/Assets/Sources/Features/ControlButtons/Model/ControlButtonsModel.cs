using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Unity.Properties;

namespace Sources.Features.ControlButtons.Model
{
    public class ControlButtonsModel : BindableObject, IViewModelDataSource
    {
        
        private readonly ILocalizationService _localizationService;

        private string _nextLanguageLabel;
        [CreateProperty]
        public string NextLanguageLabel
        {
            get => _nextLanguageLabel;
            private set { _nextLanguageLabel = value; Notify(); }
        }

        public ControlButtonsModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += FetchModel;
            FetchModel();
        }

        public void FetchModel()
        {
            NextLanguageLabel = _localizationService.GetNextLanguageTag().ToUpper();
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