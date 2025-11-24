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
        
        private string _stat1;
        [CreateProperty]
        public string Stat1
        {
            get => _stat1;
            private set 
            { 
                _stat1 = value; 
                Notify(); 
                Notify(nameof(HasStat1)); 
            }
        }

        [CreateProperty] 
        public bool HasStat1 => !string.IsNullOrEmpty(Stat1);

        private string _stat2;
        [CreateProperty]
        public string Stat2
        {
            get => _stat2;
            private set 
            { 
                _stat2 = value; 
                Notify(); 
                Notify(nameof(HasStat2)); 
            }
        }

        [CreateProperty] 
        public bool HasStat2 => !string.IsNullOrEmpty(Stat2);

        private string _stat3;
        [CreateProperty]
        public string Stat3
        {
            get => _stat3;
            private set 
            { 
                _stat3 = value; 
                Notify(); 
                Notify(nameof(HasStat3)); 
            }
        }

        [CreateProperty] 
        public bool HasStat3 => !string.IsNullOrEmpty(Stat3);

        private string _stat4;
        [CreateProperty]
        public string Stat4
        {
            get => _stat4;
            private set 
            { 
                _stat4 = value; 
                Notify(); 
                Notify(nameof(HasStat4)); 
            }
        }

        [CreateProperty] 
        public bool HasStat4 => !string.IsNullOrEmpty(Stat4);

        public PopupPresenter(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += OnLanguageChanged;
            Initialize();
        }

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
            Stat1 = _data.Stat1;
            Stat2 = _data.Stat2;
            Stat3 = _data.Stat3;
            Stat4 = _data.Stat4;
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