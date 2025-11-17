using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sources.Features.RightPopup
{
    public class RightPopupModel : BindableObject, IViewModelDataSource
    {

        private readonly ILocalizationService _localizationService;

        // private static class ContentKeys
        // {
        //     public const string TitleKey = "some-title-name-tmp-tmp-tmp";
        // }

        private string _date;

        [CreateProperty]
        public string Date
        {
            get => _date;
            private set
            {
                _date = value;
                Notify();
            }
        }

        private string _place;

        [CreateProperty]
        public string Place
        {
            get => _place;
            private set
            {
                _place = value;
                Notify();
            }
        }

        private string _text;

        [CreateProperty]
        public string Text
        {
            get => _text;
            private set
            {
                _text = value;
                Notify();
            }
        }

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

        [CreateProperty] public bool HasStat1 => !string.IsNullOrEmpty(Stat1);

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

        [CreateProperty] public bool HasStat2 => !string.IsNullOrEmpty(Stat2);

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

        [CreateProperty] public bool HasStat3 => !string.IsNullOrEmpty(Stat3);

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

        [CreateProperty] public bool HasStat4 => !string.IsNullOrEmpty(Stat4);


        public RightPopupModel(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += FetchModel;
            FetchModel();
        }

        public void FetchModel()
        {
            Date = "1180 <size=80%>p.n.e."; //_localizationService.GetTranslation(GlobeScreenModel.ContentKeys.TitleKey);
            //Place = "Rosja\n<size=50%>Rostów nad Donem"; 
            Place = "Grecja\n<size=80%>Saloniki"; //_localizationService.GetTranslation(GlobeScreenModel.ContentKeys.TitleKey);
            var text1 =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do <size=60%><i>eiusmod tempor incididunt ut labore et dolore magna aliqua. </i></size>Quis ipsum suspendisse ultrices gravida. Risus commodo viverra maecenas accumsan lacus vel facilisis.";
            var text2 =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Quis ipsum suspendisse ultrices gravida. Risus commodo viverra maecenas accumsan lacus vel facilisis.";
            Text = $"{text1}\n\n<size=90%>{text2}";

            Stat1 = "900";
            Stat2 = "500";
            Stat3 = "120";
            Stat4 = "400";

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
   