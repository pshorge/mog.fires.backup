using Artigio.MVVMToolkit.Core.MVVM.Base;
using Artigio.MVVMToolkit.Core.Navigation;
using Artigio.MVVMToolkit.Core.UI;
using Sources.Features.GlobeScreen.Model;
using Sources.Features.RightPopup;
using Sources.Presentation.Core.Types;
using Sources.Presentation.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.GlobeScreen.ViewModel
{
    public class GlobeScreenViewModel : BaseViewModel<ViewType, GlobeScreenModel>
    {

        private static class UI
        {
            // BEM class names
            public const string GlobeScreenBgName = "globe-screen__bg";
            public const string GlobeScreenRightPopupName = "right_popup";
            public const string RightPopupVisibleClass = "right-popup--visible";
        }
        
        // Model
        protected override GlobeScreenModel Model { get; set; }
        
        private RightPopupModel _rightPopupModel;
        
        private MediaBackground _media;
        private Timeline _timeline;
        private const int ScrollStep = 1;
        
        private VisualElement _rightPopup;
        private bool _popupActive;
        

        
        // Dependencies
        [Inject] private INavigationFlowController<ViewType> _navigationController;

        // Implementation
        public override ViewType GetViewType() => ViewType.Globe;
        protected override string ContainerName => "globe-screen";

        [Inject]
        public void Initialize(GlobeScreenModel model, RightPopupModel rightPopupModel)
        {
            Model = model;
            _rightPopupModel = rightPopupModel;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            Container.dataSource = Model;
            _rightPopup.dataSource = _rightPopupModel;
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
            if (Model.SelectedStartIndex != start) Model.SelectedStartIndex = start;
            if (Model.SelectedEndIndex   != end)   Model.SelectedEndIndex   = end;
            UpdateSelectionFullFlag(start, end);

        }
        private void UpdateSelectionFullFlag(int start, int end)
        {
            int n = Model.TimelinePeriods?.Count ?? 0;
            bool isFull = (n > 0 && start == n - 1 && end == n);
            if (Model.IsTimelineSelectionFull != isFull)
                Model.IsTimelineSelectionFull = isFull;
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
