using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Features.GlobeScreen.Presenter;
using Sources.Features.RightPopup.Presenter;
using Sources.Presentation.Core.Types;
using Sources.Presentation.UI.Components;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.GlobeScreen.View
{
    /// <summary>
    /// View for Globe screen
    /// Manages globe interaction and timeline UI
    /// </summary>
    public class GlobeView : BaseView<ViewType, GlobePresenter>
    {
        private static class UI
        {
            public const string GlobeScreenBgName = "globe-screen__bg";
            public const string GlobeScreenRightPopupName = "right_popup";
            public const string RightPopupVisibleClass = "right-popup--visible";
        }
        
        // Presenter reference
        protected override GlobePresenter Presenter { get; set; }
        
        private RightPopupPresenter _rightPopupPresenter;
        
        // UI Elements
        private MediaBackground _media;
        private Timeline _timeline;
        private VisualElement _rightPopup;
        private bool _popupActive;
        
        private const int ScrollStep = 1;

        // Dependencies
        [Inject] private INavigationFlowController<ViewType> _navigationController;

        // View configuration
        public override ViewType GetViewType() => ViewType.Globe;
        protected override string ContainerName => "globe-screen";

        [Inject]
        public void Initialize(GlobePresenter presenter, RightPopupPresenter rightPopupPresenter)
        {
            Presenter = presenter;
            _rightPopupPresenter = rightPopupPresenter;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            Container.dataSource = Presenter;
            _rightPopup.dataSource = _rightPopupPresenter;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null;
            _rightPopup.dataSource = null;
        }
        
        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(UI.GlobeScreenBgName);
            _timeline = Container.Q<Timeline>();
            _rightPopup = Container.Q<VisualElement>(UI.GlobeScreenRightPopupName);
        }

        private void RegisterEventHandlers()
        {
            Container.RegisterCallback<ClickEvent>(OnTouched);
            Container.RegisterCallback<WheelEvent>(OnGlobalWheel, TrickleDown.TrickleDown);
            _timeline.SelectionChanged += OnTimelineSelectionChanged;
        }

        private void UnregisterEventHandlers()
        {
            Container.UnregisterCallback<ClickEvent>(OnTouched);
            Container.UnregisterCallback<WheelEvent>(OnGlobalWheel, TrickleDown.TrickleDown);
            _timeline.SelectionChanged -= OnTimelineSelectionChanged;
        }

        private void OnTouched(ClickEvent evt)
        {
            _popupActive = !_popupActive;
            _rightPopup.EnableInClassList(UI.RightPopupVisibleClass, _popupActive);
        }
        
        private void OnTimelineSelectionChanged(int start, int end)
        {
            if (Presenter.SelectedStartIndex != start) 
                Presenter.SelectedStartIndex = start;
            if (Presenter.SelectedEndIndex != end) 
                Presenter.SelectedEndIndex = end;
        }
        
        private void OnGlobalWheel(WheelEvent evt)
        {
            if (!IsVisible) return;

            int dir = evt.delta.y < 0 ? 1 : -1;
            _timeline?.Nudge(dir * ScrollStep);
            evt.StopPropagation();
        }
        
        public void OnDialTick(int deltaTicks)
        {
            if (!IsVisible) return;
            _timeline?.Nudge(deltaTicks);
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