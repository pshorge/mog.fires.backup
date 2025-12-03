using System;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Data.Models;
using Sources.Features.ControlButtons.Presenter;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Presentation.Core.Types;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.ControlButtons.View
{
    /// <summary>
    /// View for Control Panel
    /// Manages navigation and accessibility buttons
    /// </summary>
    public class ControlPanelView : BaseView<ViewType, ControlPanelPresenter>
    {
        // UI Constants
        private static class UI
        {
            public const string LanguageButtonName = "control-panel-language-button";
            public const string ModeIconName = "control-panel-mode-icon";
            public const string GlobeIconClass = "control-panel__button--globe-icon";
            public const string MapIconClass = "control-panel__button--map-icon";

        }
        
        // Presenter reference
        protected override ControlPanelPresenter Presenter { get; set; }
        
        // UI Elements
        private Button _languageButton;
        private VisualElement _modeIcon;
        // Dependencies
        [Inject] private ILocalizationService _localizationService;
        [Inject] private INavigationFlowController<ViewType> _navigationController;
        
        // View configuration
        public override ViewType GetViewType() => ViewType.None;
        protected override string ContainerName => "control-panel";

        private IDisposable _inputSubscription;

        [Inject]
        public void Initialize(AppContent content, IUnifiedInputService inputService)
        {
            Presenter = content.ControlPanelPresenter;
            _inputSubscription = inputService.Subscribe(InputActionType.ChangeLanguage, OnLanguageChanged
            );
        }
        
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
            _inputSubscription.Dispose();
            UnregisterEventHandlers();
            Container.dataSource = null;
        }
        
        protected override void Start()
        {
            base.Start();
            Show();
        }
        
        private void SetupUIElements()
        {
            _languageButton = Container.Q<Button>(UI.LanguageButtonName);
            _modeIcon = Container.Q<VisualElement>(UI.ModeIconName);
        }

        private void RegisterEventHandlers()
        {
            _languageButton.clicked += OnLanguageChanged;
        }
        
        private void UnregisterEventHandlers()
        {
            _languageButton.clicked -= OnLanguageChanged;
        }
        
        public void EnableGlobeButton(bool enable) {
            _modeIcon.EnableInClassList(UI.GlobeIconClass, enable);
            _modeIcon.EnableInClassList(UI.MapIconClass, !enable);
        } 
        
        //public void EnableBackButton(bool value) => _backButton.visible = value;
        public void EnableButtons(bool value) => Container.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        
        private void OnLanguageChanged() => _localizationService.ChangeLanguage();
        
        public void SetDefault()
        {
            _localizationService.SetDefaultLanguage();
        }
    }
}