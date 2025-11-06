using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Navigation;
using Artigio.MVVMToolkit.Core.UI;
using Sources.Features.StartScreen.Model;
using Sources.Presentation.Core.Types;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.StartScreen.ViewModel
{
    public class GlobeScreenViewModel : BaseViewModel<ViewType, GlobeScreenModel>
    {

        private static class UI
        {
            // BEM class names
            public const string GlobeScreenBgName = "globe-screen__bg";
            
        }
        
        // Model
        protected override GlobeScreenModel Model { get; set; }
        
        private MediaBackground _media;

        
        // Dependencies
        [Inject] private INavigationFlowController<ViewType> _navigationController;

        // Implementation
        public override ViewType GetViewType() => ViewType.Globe;
        protected override string ContainerName => "globe-screen";

        [Inject]
        public void Initialize(GlobeScreenModel model)
        {
            Model = model;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            Container.dataSource = Model;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null; 
        }
        
        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(UI.GlobeScreenBgName);
        }

        private void RegisterEventHandlers()
        {
            Container.RegisterCallback<ClickEvent>(OnTouched);
        }

        private void UnregisterEventHandlers()
        {
            Container.UnregisterCallback<ClickEvent>(OnTouched);
        }

        private void OnTouched(ClickEvent evt)
        {
            _navigationController.NavigateForward();
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
    }
}
