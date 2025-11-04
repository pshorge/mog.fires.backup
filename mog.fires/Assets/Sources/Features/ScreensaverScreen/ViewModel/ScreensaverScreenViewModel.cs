using System;
using Artigio.MVVMToolkit.Core.Application.Services;
using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Navigation;
using Artigio.MVVMToolkit.Core.UI;
using Sources.Features.ScreensaverScreen.Model;
using Sources.Presentation.Core.Types;
using UnityEngine.UIElements;
using UnityEngine.Video;
using VContainer;

namespace Sources.Features.ScreensaverScreen.ViewModel
{
    public class ScreensaverScreenViewModel : BaseViewModel<ViewType, ScreensaverScreenModel>
    {
        // Constants
        private static class UI
        {
            // BEM class names
            public const string ScreensaverClass = "screensaver";
            public const string TouchPointIconClass = "screensaver__icon--touch-point";
            public const string HandIconClass = "screensaver__icon--hand";
            public const string VideoModifier = "screensaver--video";
            
            // Screen transition classes
            public const string ScreenTransitionClass = "screen--transition-top-down";
            public const string ScreenEnteringClass = "screen--entering";
            public const string ScreenVisibleClass = "screen--visible";
        }

        // Model
        protected override ScreensaverScreenModel Model { get; set; }

        // UI elements
        private VisualElement _touchPointIcon;
        private VisualElement _touchIcon;

        // Dependencies
        [Inject] private INavigationFlowController<ViewType> _navigationController;
        [Inject] private IInactivityService _inactivityService;
        [Inject] private VideoPlayer _videoPlayer;

        // Implementation
        public override ViewType GetViewType() => ViewType.Screensaver;
        protected override string ContainerName => UI.ScreensaverClass;
        
        private Action _onTopDownTransitionComplete;

        [Inject]
        public void Initialize(ScreensaverScreenModel model)
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

        protected override void Start()
        {
            base.Start();
            SetBackground();
        }

        private void SetupUIElements()
        {
            _touchIcon = Container.Q<VisualElement>(className: UI.HandIconClass);
            _touchPointIcon = Container.Q<VisualElement>(className: UI.TouchPointIconClass);
        }

        private void RegisterEventHandlers()
        {
            Container.RegisterCallback<ClickEvent>(OnTouched);
            Container.RegisterCallback<TransitionEndEvent>(HandleTopDownTransitionEnd);
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            Model.propertyChanged += OnModelPropertyChanged;
        }

        private void UnregisterEventHandlers()
        {
            Container.UnregisterCallback<ClickEvent>(OnTouched);
            Container.UnregisterCallback<TransitionEndEvent>(HandleTopDownTransitionEnd);
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            Model.propertyChanged -= OnModelPropertyChanged;
        }

        private void OnTouched(ClickEvent evt)
        {
            _navigationController.NavigateForward();
        }
        
        private void OnVideoPrepared(VideoPlayer source)
        {
            if (Container.style.display == DisplayStyle.Flex) 
                source.Play();
        }

        private void SetBackground()
        {
            var useVideo = Model.HasVideoBg;
            Container.EnableInClassList(UI.VideoModifier, useVideo);
            
            if (useVideo) return;
            
            if (_videoPlayer.enabled)
            {
                _videoPlayer.Stop();
                _videoPlayer.url = string.Empty;
                _videoPlayer.enabled = false;
            }
            SetImageElement(Container, Model.BackgroundFilePath);
        }
        
        private void OnModelPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(Model.BackgroundFilePath))
            {
                SetBackground();
            }
        }

        public override void Show()
        {
            base.Show();
            _inactivityService.StopMonitoring();
            if (Model.HasVideoBg) 
                PrepareAndPlayVideo();
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
    
            if (Model.HasVideoBg) 
                PrepareAndPlayVideo();

            Container.schedule.Execute(() => {
                Container.RemoveFromClassList(UI.ScreenEnteringClass);
                Container.AddToClassList(UI.ScreenVisibleClass);
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
            if (_videoPlayer.enabled) 
                _videoPlayer.Stop();
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
        
        private void PrepareAndPlayVideo()
        {
            _videoPlayer.enabled = true;
            _videoPlayer.url = Model.BackgroundFilePath;
            _videoPlayer.Stop();
            _videoPlayer.Prepare();
        }
    }
}