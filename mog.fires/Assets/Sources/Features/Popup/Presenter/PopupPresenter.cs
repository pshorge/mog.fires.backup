using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Features.Popup.Model;
using Unity.Properties;

namespace Sources.Features.Popup.Presenter
{
    /// <summary>
    /// Presenter for Popup panel
    /// Manages popup data and statistics
    /// </summary>
    public class PopupPresenter : BaseDataSource, IPresenter
    {
        private readonly ILocalizationService _localizationService;
        private PopupData _data;

        // Bindable properties
        private string _date;
        [CreateProperty]
        public string Date
        {
            get => _date;
            private set { _date = value; Notify(); }
        }

        private string _place;
        [CreateProperty]
        public string Place
        {
            get => _place;
            private set { _place = value; Notify(); }
        }
        
        private string _region;
        [CreateProperty]
        public string Region
        {
            get => _region;
            private set { _region = value; Notify(); }
        }
        
        private string _text;
        [CreateProperty]
        public string Text
        {
            get => _text;
            private set { _text = value; Notify(); }
        }

        private string _mediaPath;
        [CreateProperty]
        public string MediaPath
        {
            get => _mediaPath;
            private set 
            { 
                _mediaPath = value; 
                Notify(); 
                Notify(nameof(HasMediaPath)); 
            }
        }

        [CreateProperty] 
        public bool HasMediaPath => !string.IsNullOrEmpty(MediaPath);
        

        public void Initialize()
        {
            _data = new PopupData();
            UpdateBindableProperties();
        }

        public void RefreshData()
        {
            UpdateBindableProperties();
        }

        public void SetData(PopupData data)
        {
            if (data is null) return;
            _data = data;
            UpdateBindableProperties();
        }

        private void UpdateBindableProperties()
        {
            Date = _data.Date;
            Place = _data.Place;
            Region = _data.Region;
            Text = _data.Text;
            MediaPath = _data.MediaPath;
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