using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Artigio.MVVMToolkit.Core.Navigation;
using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Application.Services;
using Sources.Presentation.Core.Types;
using Sources.Features.ScreensaverScreen.ViewModel;
using Sources.Features.ControlButtons.ViewModel;
using Sources.Presentation.Management;

namespace Sources.Presentation.Navigation
{
    public class NavigationService : MonoBehaviour, INavigationFlowController<ViewType>
    {
        [SerializeField] private BaseViewModel[] viewModelComponents;
        [SerializeField] private ControlButtonsViewModel controlButtons;
        
        private Dictionary<ViewType, IViewModel<ViewType>> _views;
        private NavigationContext<ViewType> _context;
        private NavigationConfiguration _configuration;
        private IViewTransitionExecutor _transitionExecutor;
        private readonly SemaphoreSlim _navigationSemaphore = new(1, 1);
        private ControlButtonsPresenter _controlButtonsPresenter;
        
        [Inject] private IInactivityService _inactivityService;
        
        public event Action<ViewType> OnNavigated;
        public event Action<ViewType, ViewType> OnNavigating;
        public ViewType CurrentView => _context?.CurrentState?.ViewType ?? ViewType.None;
        public bool IsNavigating { get; private set; }
        
        private void Awake()
        {
            InitializeViews();
            _context = new NavigationContext<ViewType>();
            _configuration = new NavigationConfiguration();
            _transitionExecutor = new DefaultViewTransitionExecutor();
            _controlButtonsPresenter = new ControlButtonsPresenter(controlButtons);
        }
        
        private void Start()
        {
            SubscribeToEvents(true);
            NavigateTo(ViewType.Screensaver);
        }
        
        private void OnDisable()
        {
            SubscribeToEvents(false);
        }
        
        private void InitializeViews()
        {
            _views = new Dictionary<ViewType, IViewModel<ViewType>>();
            foreach (var component in viewModelComponents)
            {
                if (component is IViewModel<ViewType> viewModel)
                    _views[viewModel.GetViewType()] = viewModel;
            }
        }
        
        private void SubscribeToEvents(bool subscribe)
        {
            if (_inactivityService == null) return;
            
            if (subscribe)
            {
                _inactivityService.OnInactivityDetected += OnScreensaverTimeout;
                if (_inactivityService.IsMonitoring)
                    _inactivityService.StopMonitoring();
            }
            else
            {
                _inactivityService.OnInactivityDetected -= OnScreensaverTimeout;
            }
        }
        
        private void OnScreensaverTimeout()
        {
            NavigateTo(ViewType.Screensaver);
            controlButtons.SetDefault();
        }
        
        // INavigationFlowController implementation
        public void NavigateTo(ViewType viewType) => NavigateToAsync(viewType, null).Forget();

        public void NavigateTo<TArgs>(ViewType viewType, TArgs args) where TArgs : class => NavigateToAsync(viewType, args).Forget();

        public void NavigateBack() => NavigateBackAsync().Forget();

        public void NavigateForward() => NavigateForwardAsync().Forget();

        public void SetButtonsVisibility(bool isVisible)
        {
            controlButtons.EnableLeftButtons(isVisible);
            controlButtons.EnableRightButtons(isVisible);
            controlButtons.EnableBackButton(isVisible);
        }
        
        public void RefreshUI()
        {
            _controlButtonsPresenter.ConfigureFor(CurrentView);
        }
        
        // Navigation methods
        private async UniTaskVoid NavigateBackAsync()
        {
            var node = _configuration.GetNode(CurrentView);
            if (node?.AllowBack != true) return;
            
            var previousView = node.PreviousView;
            if (previousView.HasValue)
                await NavigateToAsync(previousView.Value, null);
        }
        
        private async UniTaskVoid NavigateForwardAsync()
        {
            var node = _configuration.GetNode(CurrentView);
            if (node?.AllowForward != true) return;
            
            var nextView = node.NextView;
            if (nextView.HasValue)
            {
                object args = null;
                
                // Special handling for specific views
                // if (nextView == ViewType.Intro)
                //     args = new GameInitNavigationArgs(0);
                
                await NavigateToAsync(nextView.Value, args);
            }
        }
        
        private async UniTask NavigateToAsync(ViewType targetView, object args)
        {
            if (IsNavigating)
            {
                Debug.LogWarning($"Already navigating, ignoring request to navigate to {targetView}");
                return;
            }
    
            if (CurrentView == targetView)
            {
                Debug.Log($"Already at {targetView}, ignoring navigation request");
                return;
            }
    
            await _navigationSemaphore.WaitAsync();
            try
            {
                IsNavigating = true;
                Debug.Log($"Starting navigation from {CurrentView} to {targetView}");
                OnNavigating?.Invoke(CurrentView, targetView);
        
                var fromState = _context.CurrentState;
                var toState = new NavigationState(targetView, args);
        
        
                // Special handling for screensaver transition
                if (targetView == ViewType.Screensaver && fromState != null && 
                    fromState.ViewType != ViewType.None)
                {
                    Debug.Log("Using top-down transition for screensaver");
                    _controlButtonsPresenter.ConfigureFor(targetView);
                    await ShowScreensaverWithTopDownTransition(fromState.ViewType);
                }
                else
                {
                    Debug.Log("Using standard transition");
                    await PerformStandardTransition(fromState, toState);
                    _controlButtonsPresenter.ConfigureFor(targetView);
                }
        
                _context.PushState(toState);
                OnNavigated?.Invoke(targetView);
        
                Debug.Log($"Navigation complete: {targetView}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Navigation error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                IsNavigating = false;
                _navigationSemaphore.Release();
            }
        }
        
        
        private async UniTask PerformStandardTransition(INavigationState<ViewType> from, 
            INavigationState<ViewType> to)
        {
            var fromView = from != null ? _views.GetValueOrDefault(from.ViewType) : null;
            var toView = _views.GetValueOrDefault(to.ViewType);
    
            if (toView == null) return;
    
            // Hide all views first
            HideAllViews();
    
            await UniTask.Delay(50);
    
            // Apply navigation args if needed
            if (to.NavigationArgs != null)
            {
                ApplyNavigationArgs(to.ViewType, to.NavigationArgs);
            }
    
            // Show target view
            toView.Show();
    
            // Handle inactivity service
            if (to.ViewType == ViewType.Screensaver)
                _inactivityService?.StopMonitoring();
            else
                _inactivityService?.StartMonitoring();
        }

        private async UniTask ShowScreensaverWithTopDownTransition(ViewType fromViewType)
        {
            var screensaverVm = _views[ViewType.Screensaver] as ScreensaverScreenViewModel;
            var fromView = _views.GetValueOrDefault(fromViewType);
    
            if (screensaverVm == null) return;
    
            //_controlButtonsPresenter.ConfigureFor(ViewType.Screensaver);
            var tcs = new UniTaskCompletionSource();
    
            screensaverVm.ShowTopDownScreen(() => 
            {
                fromView?.Hide();
                tcs.TrySetResult();
            });
    
            await tcs.Task;
            await UniTask.Delay(100);
        }
        
        private void HideAllViews()
        {
            StopAllCoroutines();
            foreach (var view in _views.Values)
            {
                if (view is not null && view.IsVisible)
                    view.Hide();
            }
        }
        
        private void ApplyNavigationArgs(ViewType viewType, object args)
        {
        }
        
        // Simple transition executor (can be extended later)
        private class DefaultViewTransitionExecutor : IViewTransitionExecutor
        {
            public async UniTask ExecuteTransitionAsync(IViewModel<ViewType> from, 
                IViewModel<ViewType> to, ViewTransition transition)
            {
                if (from is { IsVisible: true })
                {
                    from.Hide();
                    await UniTask.Delay(100);
                }
                
                to?.Show();
                
                if (transition.Duration > 0)
                    await UniTask.Delay((int)(transition.Duration * 1000));
            }
        }
        
        
#if UNITY_EDITOR

        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.Alpha1))
                NavigateTo(ViewType.Screensaver);
            if(Input.GetKeyUp(KeyCode.Return) ||  Input.GetKeyUp(KeyCode.Space) ||  Input.GetKeyUp(KeyCode.Alpha2))
                NavigateForward();
        }

#endif
        
    }
}