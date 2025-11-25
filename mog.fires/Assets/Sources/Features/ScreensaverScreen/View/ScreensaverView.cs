using System;
using Psh.MVPToolkit.Core.Application.Services;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Features.ScreensaverScreen.Presenter;
using Sources.Presentation.Core.Types;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.ScreensaverScreen.View
{
    /// <summary>
    /// View for Screensaver screen
    /// Handles UI interactions and animations
    /// </summary>
    public class ScreensaverView : BaseView<ViewType, ScreensaverPresenter>
    {
        // UI Constants
        private static class UI
        {
            public const string ScreensaverClass = "screensaver";
            public const string TouchPointIconClass = "screensaver__icon--touch-point";
            public const string HandIconClass = "screensaver__icon--hand";
            public const string ScreenTransitionClass = "screen--transition-top-down";
            public const string ScreenEnteringClass = "screen--entering";
            public const string ScreenVisibleClass = "screen--visible";
        }

        // Presenter reference
        protected override ScreensaverPresenter Presenter { get; set; }

        // UI Elements
        private VisualElement _touchPointIcon;
        private VisualElement _touchIcon;
        private MediaBackground _media;

        // Dependencies
        [Inject] private INavigationFlowController<ViewType> _navigationController;
        [Inject] private IInactivityService _inactivityService;

        // View configuration
        public override ViewType GetViewType() => ViewType.Screensaver;
        protected override string ContainerName => UI.ScreensaverClass;
        
        private Action _onTopDownTransitionComplete;

        [Inject]
        public void Initialize(ScreensaverPresenter presenter)
        {
            Presenter = presenter;
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

        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(className: UI.ScreensaverClass);
            _touchIcon = Container.Q<VisualElement>(className: UI.HandIconClass);
            _touchPointIcon = Container.Q<VisualElement>(className: UI.TouchPointIconClass);
        }

        private void RegisterEventHandlers()
        {
            Container.RegisterCallback<TransitionEndEvent>(HandleTopDownTransitionEnd);
            Presenter.propertyChanged += OnPresenterPropertyChanged;
        }

        private void UnregisterEventHandlers()
        {
            Container.UnregisterCallback<TransitionEndEvent>(HandleTopDownTransitionEnd);
            Presenter.propertyChanged -= OnPresenterPropertyChanged;
        }
        
        private void OnPresenterPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            // Handle specific property changes if needed
        }

        public override void Show()
        {
            base.Show();
            _media?.Play();
            _inactivityService.StopMonitoring();
            SetTouchIconAnimationEnabled(true);
        }
        
        public void ShowTopDownScreen(Action onComplete = null)
        {
            if (Container == null) 
                return;

            _onTopDownTransitionComplete = onComplete;
            _inactivityService.StopMonitoring();
    
            IsVisible = true;
    
            Container.BringToFront();
            Container.EnableInClassList(UI.ScreenTransitionClass, true);
            Container.AddToClassList(UI.ScreenEnteringClass);
            Container.style.display = DisplayStyle.Flex;
            Container.style.opacity = 1f;
    
            Container.schedule.Execute(() => {
                Container.RemoveFromClassList(UI.ScreenEnteringClass);
                Container.AddToClassList(UI.ScreenVisibleClass);
                _media?.Play();
            }).StartingIn(50);
        }

        private void HandleTopDownTransitionEnd(TransitionEndEvent evt)
        {
            if (!evt.stylePropertyNames.Contains("translate")) 
                return;
    
            SetTouchIconAnimationEnabled(true);
    
            _onTopDownTransitionComplete?.Invoke();
            _onTopDownTransitionComplete = null;
        }
        
        public override void Hide()
        {
            base.Hide();
            _inactivityService.StartMonitoring();
            _media?.Pause();
            SetTouchIconAnimationEnabled(false);
            Container.EnableInClassList(UI.ScreenVisibleClass, false);
            Container.EnableInClassList(UI.ScreenTransitionClass, false);
        }
        
        private void SetTouchIconAnimationEnabled(bool enable)
        {
            string prefixId = GetType().ToString().ToLower();
            string fadeTouchIconAnimId = $"{prefixId}-touch-icon-fade";
            string moveTouchIconAnimId = $"{prefixId}-touch-icon-scale";
            string fadeTouchPointIconAnimId = $"{prefixId}-touch-point-icon-fade";
            
            if (enable)
            {
                _touchIcon.style.bottom = StyleKeyword.Initial;
                _touchIcon.style.opacity = 1f;
                _touchPointIcon.style.opacity = 1f;
                
                VisualElementAnimator.AnimateFade(_touchIcon, 1f, 0.7f, 1.5f, fadeTouchIconAnimId);
                VisualElementAnimator.AnimateMove(_touchIcon, 25, 1.5f, moveTouchIconAnimId);
                VisualElementAnimator.AnimateFade(_touchPointIcon, 1f, 0.7f, 1.5f, fadeTouchPointIconAnimId);
            }
            else
            {
                VisualElementAnimator.StopAnimation(fadeTouchIconAnimId);
                VisualElementAnimator.StopAnimation(moveTouchIconAnimId);
                VisualElementAnimator.StopAnimation(fadeTouchPointIconAnimId);
                
                _touchIcon.style.bottom = StyleKeyword.Initial;
                _touchIcon.style.opacity = 1f;
                _touchPointIcon.style.opacity = 1f;
            }
        }
        
        private void DismissScreensaver()
        {
            //to invoke only once
            if (!IsVisible) return;
            _navigationController.NavigateForward();
        }

        protected override void Update()
        {
            base.Update();
            if (!IsVisible) return;

            bool inputDetected = Input.anyKeyDown || 
                                 Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f ||
                                 Mathf.Abs(Input.GetAxis("Mouse X")) > 0.5f ||
                                 Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.5f;

            if (inputDetected)
            {
                DismissScreensaver();
            }
        }
    }
}