using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Psh.MVPToolkit.Core.Application.Services;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Navigation;
using Sources.Data.Models;
using Sources.Features.ControlButtons.View;
using Sources.Features.ScreensaverScreen.View;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Presentation.Core.Types;
using Sources.Presentation.Management;
using UnityEngine;
using VContainer;

namespace Sources.Presentation.Navigation
{
    public class NavigationService : MonoBehaviour, INavigationFlowController<ViewType>
    {
        [SerializeField] private BaseView[] viewComponents;
        [SerializeField] private ControlPanelView controlPanel;

        private Dictionary<ViewType, IView<ViewType>> _views;
        private NavigationContext<ViewType> _context;
        private NavigationConfiguration _configuration;
        private IViewTransitionExecutor _transitionExecutor;
        private readonly SemaphoreSlim _navigationSemaphore = new(1, 1);
        private ControlPanelManager _controlPanelManager;

        //Dependencies
        private IInactivityService _inactivityService;
        private IUnifiedInputService _inputService;
        private bool _screensaverEnabled;
        
        private readonly List<IDisposable> _inputSubscriptions = new();
        
        public event Action<ViewType> OnNavigated;
        public event Action<ViewType, ViewType> OnNavigating;
        public ViewType CurrentView => _context?.CurrentState?.ViewType ?? ViewType.None;
        public bool IsNavigating { get; private set; }

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            InitializeViews();
            _context = new NavigationContext<ViewType>();
            _transitionExecutor = new DefaultViewTransitionExecutor();
            _controlPanelManager = new ControlPanelManager(controlPanel);
        }
        
        [Inject]
        public void Construct(
            AppSettings settings, 
            IUnifiedInputService inputService,
            IInactivityService  inactivityService)
        {
            _screensaverEnabled = settings.ScreensaverEnabled;
            _configuration = new NavigationConfiguration(settings.ScreensaverEnabled);
            _inputService =  inputService;
            _inactivityService = inactivityService;
        }

        private void Start()
        {
            SubscribeToEvents(true);
            SubscribeToInput();
            NavigateTo(ViewType.Screensaver);
        }

        private void OnDisable()
        {
            SubscribeToEvents(false);
            UnsubscribeInput();
        }

        private void InitializeViews()
        {
            _views = new Dictionary<ViewType, IView<ViewType>>();
            foreach (var component in viewComponents)
            {
                if (component is IView<ViewType> view)
                    _views[view.GetViewType()] = view;
            }
        }

        private void SubscribeToInput()
        {
            if (_inputService == null) return;

            // R -> Screensaver
            _inputSubscriptions.Add(_inputService.Subscribe(InputActionType.NavigateHome, () => NavigateTo(ViewType.Screensaver)));
            
            // Space -> Forward
            _inputSubscriptions.Add(_inputService.Subscribe(InputActionType.NavigateForward, NavigateForward));
            
            // Backspace -> Back
            _inputSubscriptions.Add(_inputService.Subscribe(InputActionType.NavigateBack, NavigateBack));

            // M / SPM -> Switch Globe/Map
            _inputSubscriptions.Add(_inputService.Subscribe(InputActionType.SwitchMode, () =>
            {
                switch (CurrentView)
                {
                    case ViewType.Globe:
                        NavigateTo(ViewType.Map);
                        break;
                    case ViewType.Map:
                        NavigateTo(ViewType.Globe);
                        break;
                }
            }));
        }

        private void UnsubscribeInput()
        {
            foreach (var sub in _inputSubscriptions) sub.Dispose();
            _inputSubscriptions.Clear();
        }
        
        private void SubscribeToEvents(bool subscribe)
        {
            if (_inactivityService == null || !_screensaverEnabled) return;

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
            controlPanel.SetDefault();
        }

        // INavigationFlowController implementation
        public void NavigateTo(ViewType viewType) => NavigateToAsync(viewType, null).Forget();
        public void NavigateTo<TArgs>(ViewType viewType, TArgs args) where TArgs : class => NavigateToAsync(viewType, args).Forget();
        public void NavigateBack() => NavigateBackAsync().Forget();
        public void NavigateForward() => NavigateForwardAsync().Forget();

        public void SetButtonsVisibility(bool isVisible)
        {
            controlPanel.EnableButtons(isVisible);
        }

        public void RefreshUI()
        {
            _controlPanelManager.ConfigureFor(CurrentView);
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
                await NavigateToAsync(nextView.Value, null);
            }
        }

        private async UniTask NavigateToAsync(ViewType targetView, object args)
        {
            if (IsNavigating)
            {
                Debug.LogWarning($"Already navigating, ignoring request to navigate to {targetView}");
                return;
            }

            if (targetView == ViewType.Screensaver && !_screensaverEnabled)
            {
                targetView = ViewType.Globe;
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
                    _controlPanelManager.ConfigureFor(targetView);
                    await ShowScreensaverWithTopDownTransition(fromState.ViewType);
                }
                else
                {
                    Debug.Log("Using standard transition");
                    await PerformStandardTransition(fromState, toState);
                    _controlPanelManager.ConfigureFor(targetView);
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
            if (_screensaverEnabled)
            {
                if (to.ViewType == ViewType.Screensaver)
                    _inactivityService?.StopMonitoring();
                else
                    _inactivityService?.StartMonitoring();
            }
            else
            {
                _inactivityService?.StopMonitoring();
            }
        }

        private async UniTask ShowScreensaverWithTopDownTransition(ViewType fromViewType)
        {
            var screensaverView = _views[ViewType.Screensaver] as ScreensaverView;
            var fromView = _views.GetValueOrDefault(fromViewType);

            if (screensaverView == null) return;

            var tcs = new UniTaskCompletionSource();

            screensaverView.ShowTopDownScreen(() =>
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
            // Handle navigation args if needed
        }

        // Transition executor
        private class DefaultViewTransitionExecutor : IViewTransitionExecutor
        {
            public async UniTask ExecuteTransitionAsync(IView<ViewType> from,
                IView<ViewType> to, ViewTransition transition)
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
        
        private void Update()
        {
            if (Application.isFocused && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}