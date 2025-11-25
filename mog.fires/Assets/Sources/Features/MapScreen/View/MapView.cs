using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Features.MapScreen.Presenter;
using Sources.Presentation.Core.Types;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.MapScreen.View
{
    public class MapView : BaseView<ViewType, MapPresenter>
    {
        private static class UI
        {
            public const string MapScreenBgName = "map-screen__bg";
        }
        
        // UI Elements
        private MediaBackground _media;
       
        
        // Dependencies
        [Inject] protected override MapPresenter Presenter { get; set; }
        [Inject] private INavigationFlowController<ViewType> _navigationController;

        // View configuration
        public override ViewType GetViewType() => ViewType.Map;
        protected override string ContainerName => "map-screen";
    
        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            Container.dataSource = Presenter;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null;
        }
        
        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(UI.MapScreenBgName);
        }

        private void RegisterEventHandlers()
        {
            Presenter.propertyChanged += OnPresenterPropertyChanged;
        }

        private void UnregisterEventHandlers()
        {
            Presenter.propertyChanged -= OnPresenterPropertyChanged;
        }
        
        private void OnPresenterPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
        }

        public override void Show()
        {
            base.Show();
            _media?.Play();
        }
        
        public override void Hide()
        {
            base.Hide();
            _media?.Pause();
        }

        protected override void Update()
        {
            base.Update();
            if (!IsVisible) return;
            if (Input.GetMouseButtonDown(2)) 
                _navigationController.NavigateTo(ViewType.Globe);
        }
    }
}