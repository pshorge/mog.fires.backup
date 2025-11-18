using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.Services.Accessibility.HighContrast;
using Psh.MVPToolkit.Core.Services.Accessibility.TextResize;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Data.Models;
using Sources.Features.ControlButtons.Presenter;
using Sources.Presentation.Core.Types;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.ControlButtons.View
{
    /// <summary>
    /// View for Control Panel
    /// Manages navigation and accessibility buttons
    /// </summary>
    public class ControlPanelView : BaseView<ViewType, ControlPanelPresenter>, IResizable
    {
        // UI Constants
        private static class UI
        {
            public const string ControlButtonsLeftClass = "control-panel__section--left";
            public const string ControlButtonsRightClass = "control-panel__section--right";
            public const string HomeButtonName = "control-panel-home-button";
            public const string BackButtonName = "control-panel-back-button";
            public const string LanguageButtonName = "control-panel-language-button";
            public const string ResizeButtonName = "control-panel-resize-button";
            public const string ContrastButtonName = "control-panel-contrast-button";
        }
        
        // Presenter reference
        protected override ControlPanelPresenter Presenter { get; set; }
        
        // UI Elements
        private VisualElement _leftButtons;
        private Button _homeButton;
        private Button _backButton;
        
        private VisualElement _rightButtons;
        private Button _languageButton;
        private Button _resizeButton;
        private Button _contrastButton;
        
        // Dependencies
        [Inject] private ILocalizationService _localizationService;
        [Inject] private ITextResizeService _textResizeService;
        [Inject] private IHighContrastService _contrastService;
        [Inject] private INavigationFlowController<ViewType> _navigationController;
        
        // View configuration
        public override ViewType GetViewType() => ViewType.None;
        protected override string ContainerName => "control-panel";

        [Inject]
        public void Initialize(AppContent content)
        {
            Presenter = content.ControlPanelPresenter;
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
            _leftButtons = Container.Q<VisualElement>(className: UI.ControlButtonsLeftClass);
            _rightButtons = Container.Q<VisualElement>(className: UI.ControlButtonsRightClass);
            
            _homeButton = _leftButtons.Q<Button>(UI.HomeButtonName);
            _backButton = _leftButtons.Q<Button>(UI.BackButtonName);
            _languageButton = _rightButtons.Q<Button>(UI.LanguageButtonName);
            _resizeButton = _rightButtons.Q<Button>(UI.ResizeButtonName);
            _contrastButton = _rightButtons.Q<Button>(UI.ContrastButtonName);
        }

        private void RegisterEventHandlers()
        {
            _textResizeService.RegisterResizableTextObject(this);
            _backButton.clicked += GoBack;
            _homeButton.clicked += GoHome;
            _languageButton.clicked += OnLanguageChanged;
            _resizeButton.clicked += SwitchScale;
            _contrastButton.clicked += SwitchContrast;
        }
        
        private void UnregisterEventHandlers()
        {
            _textResizeService.UnregisterResizableTextObject(this);
            _backButton.clicked -= GoBack;
            _homeButton.clicked -= GoHome;
            _languageButton.clicked -= OnLanguageChanged;
            _resizeButton.clicked -= SwitchScale;
            _contrastButton.clicked -= SwitchContrast;
        }
        
        public void Resize(bool maximized)
        {
            if (_resizeButton == null) return;
            _resizeButton.text = maximized ? "A<smallcaps>a</smallcaps>" : "<smallcaps>a</smallcaps>A";
        }
        
        public void EnableBackButton(bool value) => _backButton.visible = value;
        public void EnableLeftButtons(bool value) => _leftButtons.visible = value;
        public void EnableRightButtons(bool value) => _rightButtons.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        
        private void GoHome() => _navigationController.NavigateTo(ViewType.Globe);
        private void GoBack() => _navigationController.NavigateBack();
        private void OnLanguageChanged() => _localizationService.ChangeLanguage();
        private void SwitchScale() => _textResizeService.Resize(!_textResizeService.Maximized);
        private void SwitchContrast() => _contrastService.SwitchContrast(!_contrastService.ContrastEnabled);
        
        public void SetDefault()
        {
            _localizationService.SetDefaultLanguage();
            if (_textResizeService.Maximized) 
                SwitchScale();
            if(_contrastService.ContrastEnabled)
                SwitchContrast();
        }
    }
}