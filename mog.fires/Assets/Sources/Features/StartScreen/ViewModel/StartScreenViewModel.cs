using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Navigation;
using Sources.Features.StartScreen.Model;
using Sources.Presentation.Core.Types;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.StartScreen.ViewModel
{
    public class StartScreenViewModel : BaseViewModel<ViewType, StartScreenModel>
    {
        
        // Model
        protected override StartScreenModel Model { get; set; }
        

        
        // Dependencies
        [Inject] private INavigationFlowController<ViewType> _navigationController;

        // Implementation
        public override ViewType GetViewType() => ViewType.Start;
        protected override string ContainerName => "start-screen";

        [Inject]
        public void Initialize(StartScreenModel model)
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

        protected override void Start()
        {
            base.Start();
            SetImages();
        }

        

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null; 
        }
        
        private void SetupUIElements()
        {
        }

        private void RegisterEventHandlers()
        {
            Container.RegisterCallback<ClickEvent>(OnTouched);
            Model.propertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            switch (e.propertyName)
            {
                case nameof(Model.BackgroundImagePath):
                    SetBackgroundImage();
                    break;
            }
        }

        private void UnregisterEventHandlers()
        {
            Container.UnregisterCallback<ClickEvent>(OnTouched);
            Model.propertyChanged -= OnModelPropertyChanged;
        }

        private void OnTouched(ClickEvent evt) => _navigationController.NavigateForward();
        private void SetBackgroundImage() => SetImageElement(Container, Model.BackgroundImagePath);
        private void SetImages()
        {
            SetBackgroundImage();
        }
    }
}
