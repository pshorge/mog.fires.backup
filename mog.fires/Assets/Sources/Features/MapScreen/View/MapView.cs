using System;
using System.Collections.Generic;
using System.Linq;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Data.Models; 
using Sources.Features.MapScreen.Presenter;
using Sources.Features.Popup.Presenter;
using Sources.Infrastructure;
using Sources.Infrastructure.Configuration;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Presentation.Core.Types;
using Sources.Presentation.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Sources.Features.MapScreen.View
{
    public class MapView : BaseView<ViewType, MapPresenter>
    {
        private static class UI
        {
            public const string MapScreenBgName = "map-screen__bg";
            public const string MapScreenMapName = "map-screen__map";
            public const string MapScreenMarkersClass = "map-screen__markers";
            public const string CrosshairName = "crosshair";
            
            public const string MapScreenLeftPopupName = "left_popup";
            public const string MapScreenRightPopupName = "right_popup";
            public const string LeftPopupVisibleClass = "left-popup--visible";
            public const string RightPopupVisibleClass = "right-popup--visible";
            public const string MapScreenDisambiguationMenuName = "map-screen__disambiguation_menu";
        }
        
        // UI Elements
        private MediaBackground _media;
        private MediaBackground _map;
        private VisualElement _markersContainer;
        private CrosshairElement _crosshair;
        private Timeline _timeline;
        private VisualElement _leftPopup;
        private VisualElement _rightPopup;
        private DisambiguationMenu _disambiguationMenu;
        
        private readonly List<MapMarkerElement> _activeMarkers = new();
        
        private List<MapMarkerElement> _currentCandidates = new();
        private InteractionState _state = InteractionState.Roaming;
        private float _menuScrollAccumulator = 0f;

        // Dependencies
        protected override MapPresenter Presenter { get; set; }
        private PopupPresenter _popupPresenter;
        private MapController _mapController; 
        private IUnifiedInputService _inputService;
        private MapConfig _mapConfig;
        private InputConfig _inputConfig;

        public override ViewType GetViewType() => ViewType.Map;
        protected override string ContainerName => "map-screen";
        
        
        [Inject]
        public void Construct(
            AppConfig appConfig, 
            MapPresenter presenter, 
            PopupPresenter popupPresenter, 
            MapController mapController,
            IUnifiedInputService inputService)
        {
            _mapConfig = appConfig.Map;
            _inputConfig = appConfig.Input;
            Presenter = presenter;
            _popupPresenter = popupPresenter;
            _mapController = mapController;
            _inputService = inputService;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            
            Container.dataSource = Presenter;
            if(_leftPopup != null) _leftPopup.dataSource = _popupPresenter;
            if(_rightPopup != null) _rightPopup.dataSource = _popupPresenter;
            
            RebuildMarkers(); 
            
            if (_crosshair != null) _crosshair.style.opacity = 0;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null;
            if(_leftPopup != null) _leftPopup.dataSource = null;
            if(_rightPopup != null) _rightPopup.dataSource = null;
            ClearMarkers();
        }
        
        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(UI.MapScreenBgName);
            _map = Container.Q<MediaBackground>(UI.MapScreenMapName);
            _timeline = Container.Q<Timeline>();
            _markersContainer = Container.Q<VisualElement>(className: UI.MapScreenMarkersClass);
            _crosshair = Container.Q<CrosshairElement>(UI.CrosshairName);
            
            _leftPopup = Container.Q<VisualElement>(UI.MapScreenLeftPopupName);
            _rightPopup = Container.Q<VisualElement>(UI.MapScreenRightPopupName);
            _disambiguationMenu = Container.Q<DisambiguationMenu>(UI.MapScreenDisambiguationMenuName);
            
            _map?.RegisterCallback<GeometryChangedEvent>(OnMapGeometryChanged);
        }

        private void RegisterEventHandlers()
        {
            _inputService.OnAction += HandleInput;
            if (_timeline != null) _timeline.SelectionChanged += OnTimelineSelectionChanged;
            Presenter.propertyChanged += OnPresenterPropertyChanged;
        }

        private void UnregisterEventHandlers()
        {
            _inputService.OnAction -= HandleInput;
            if (_timeline != null) _timeline.SelectionChanged -= OnTimelineSelectionChanged;
            Presenter.propertyChanged -= OnPresenterPropertyChanged;
            _map?.UnregisterCallback<GeometryChangedEvent>(OnMapGeometryChanged);
        }
        
        private void OnPresenterPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(Presenter.VisiblePoints)) RebuildMarkers();
        }

        private void OnMapGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateMarkersPosition();
        }
        
        private void HandleInput(InputActionType action)
        {
            if (!IsVisible) return;

            switch (action)
            {
                case InputActionType.Select:
                    OnActionSelect();
                    break;
                case InputActionType.Back:
                    OnActionBack();
                    break;
            }
        }

        private void OnActionSelect()
        {
            switch (_state)
            {
                case InteractionState.Roaming:
                {
                    if(_currentCandidates.Count > 1)
                    {
                        SetState(InteractionState.Disambiguation);
                    }
                    else  if(_currentCandidates.Count == 1)
                    {
                        SetState(InteractionState.Details);
                        TogglePopup(true, _currentCandidates[0].Data);
                    }
                    break;
                }
                case InteractionState.Disambiguation:
                {
                    var selected = _disambiguationMenu.GetSelectedItem();
                    if (selected != null)
                    {
                        SetState(InteractionState.Details);
                        TogglePopup(true, selected);
                    }
                    break;
                }
                case InteractionState.Details:
                    SetState(InteractionState.Roaming);
                    break;
            }
        }

        private void OnActionBack()
        {
            switch (_state)
            {
                case InteractionState.Roaming:
                    break;
                case InteractionState.Details when _currentCandidates.Count > 1:
                    SetState(InteractionState.Disambiguation);
                    break;
                case InteractionState.Disambiguation or InteractionState.Details :
                    SetState(InteractionState.Roaming);
                    break;
            }
        }
        
        private void RebuildMarkers()
        {
            ClearMarkers();
            if (Presenter.VisiblePoints == null) return;
            
            foreach (var pointData in Presenter.VisiblePoints)
            {
                var marker = new MapMarkerElement(pointData);
                _markersContainer.Add(marker);
                _activeMarkers.Add(marker);
            }
            UpdateMarkersPosition();
        }

        private void ClearMarkers()
        {
            foreach (var marker in _activeMarkers) { marker.Dispose(); marker.RemoveFromHierarchy(); }
            _activeMarkers.Clear();
        }

        private void UpdateMarkersPosition()
        {
            if (_activeMarkers.Count == 0 || _map == null) return;

            float currentW = _map.contentRect.width;
            float currentH = _map.contentRect.height;

            // If layout is not ready, abort.
            if (float.IsNaN(currentW) || currentW <= 1f) return;

            // Scale Factor: Current Size / Original Image Size
            float scaleX = currentW / _mapConfig.OriginalWidth;
            float scaleY = currentH / _mapConfig.OriginalHeight;

            foreach (var marker in _activeMarkers)
            {
                var originalPixelPos = LatLonToOriginalPixelPos(marker.Data.Latitude, marker.Data.Longitude);
                
                // Apply scale
                float uiX = originalPixelPos.x * scaleX;
                float uiY = originalPixelPos.y * scaleY;

                // Check bounds
                if (uiX >= 0 && uiX <= currentW && uiY >= 0 && uiY <= currentH)
                {
                    marker.style.display = DisplayStyle.Flex;
                    marker.style.left = uiX;
                    marker.style.top = uiY;
                }
                else
                {
                    marker.style.display = DisplayStyle.None;
                }
            }
        }
        
        private Vector2 LatLonToOriginalPixelPos(double lat, double lon)
        {
            var r1 = _mapConfig.Ref1;
            var r2 = _mapConfig.Ref2;

            var scaleX = (r2.X - r1.X) / (r2.Lon - r1.Lon);
            var scaleY = (r2.Y - r1.Y) / (r2.Lat - r1.Lat);
            
            return new Vector2(
                (float)(r1.X + ((lon - r1.Lon) * scaleX)), 
                (float)(r1.Y + ((lat - r1.Lat) * scaleY)));
        }

        // --- State Machine ---

        private void SetState(InteractionState newState)
        {
            _state = newState;
            Debug.Log($"[MapView] State changed to: {newState}");

            switch (newState)
            {
                case InteractionState.Roaming:
                    if(IsVisible) _mapController?.SetInputActive(true);
                    TogglePopup(false); 
                    _disambiguationMenu?.Hide();
                    ClearHighlights();
                    break;

                case InteractionState.Disambiguation:
                    _mapController?.SetInputActive(false);
                    TogglePopup(false);
                    _disambiguationMenu?.Show(_currentCandidates.Select(marker => marker.Data).ToList());
                    AlignMenuWithCrosshair();
                    break;

                case InteractionState.Details:
                    _mapController?.SetInputActive(false);
                    _disambiguationMenu?.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void TogglePopup(bool show, PointData data = null)
        {
            var enablePopup = show && data is not null;
            if (enablePopup) _popupPresenter.SetData(data.ToPopupData());
            
            if (_leftPopup != null) _leftPopup.EnableInClassList(UI.LeftPopupVisibleClass, enablePopup);
            if (_rightPopup != null) _rightPopup.EnableInClassList(UI.RightPopupVisibleClass, enablePopup);
        }
        
        private void AlignMenuWithCrosshair()
        {
            if (_disambiguationMenu == null || _mapController == null) return;

            var pos = _mapController.PositionNormalized;

            _disambiguationMenu.style.left = Length.Percent(pos.x * 100f);
            _disambiguationMenu.style.top = Length.Percent(pos.y * 100f);
            _disambiguationMenu.style.marginLeft = Length.Pixels(100);
            
        }

        // --- Lifecycle ---

        public override void Show()
        {
            base.Show();
            _media?.Play();
            _mapController?.ResetPosition();
            if (_crosshair != null) _crosshair.style.opacity = 1;
            
            SetState(InteractionState.Roaming);
            Container.schedule.Execute(UpdateMarkersPosition).StartingIn(50);
        }
        
        public override void Hide()
        {
            base.Hide();
            _media?.Pause();
            _mapController?.SetInputActive(false);
            TogglePopup(false);
            if (_crosshair != null) _crosshair.style.opacity = 0;
        }

        protected override void Update()
        {
            base.Update();
            if (!IsVisible) return;

            UpdateCrosshairFromController();

            switch (_state)
            {
                case InteractionState.Roaming:
                    UpdateCandidateSelection();
                    HandleTimelineInput(); 
                    break;
                    
                case InteractionState.Disambiguation:
                    HandleMouseScrollMenu();
                    break;
            }
            
        }


        private void UpdateCrosshairFromController()
        {
            if (_crosshair == null || _mapController == null) return;
            Vector2 normPos = _mapController.PositionNormalized;
            _crosshair.style.left = Length.Percent(normPos.x * 100f);
            _crosshair.style.top = Length.Percent(normPos.y * 100f);
        }

        private void UpdateCandidateSelection()
        {
            if (_activeMarkers.Count == 0 || _crosshair == null) return;

            Vector2 crosshairPos = new Vector2(_crosshair.layout.x, _crosshair.layout.y);
    
            var newCandidates = new List<MapMarkerElement>();

            foreach (var marker in _activeMarkers)
            {
                if (marker.style.display == DisplayStyle.None) continue;

                Vector2 markerPos = new Vector2(marker.layout.x, marker.layout.y);
                float dist = Vector2.Distance(crosshairPos, markerPos);
        
                if (dist <= _inputConfig.SelectionThresholdPixels)
                {
                    newCandidates.Add(marker);
                }
            }

            newCandidates.Sort((a, b) =>
            {
                float distA = Vector2.Distance(crosshairPos, new Vector2(a.layout.x, a.layout.y));
                float distB = Vector2.Distance(crosshairPos, new Vector2(b.layout.x, b.layout.y));
                return distA.CompareTo(distB);
            });

            bool changed = _currentCandidates.Count != newCandidates.Count;
            if (!changed)
            {
                for (int i = 0; i < newCandidates.Count; i++)
                {
                    if (_currentCandidates[i] != newCandidates[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                ClearHighlights();
                _currentCandidates = newCandidates;
                foreach (var c in _currentCandidates) c.SetHighlight(true);
            }
        }
        
        private void HandleTimelineInput()
        {
            var scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) < 0.1f) return;
            
            int dir = scrollDelta < 0 ? 1 : -1;
            _timeline?.Nudge(dir * _inputConfig.ScrollStep);
        }
        
        private void HandleMouseScrollMenu()
        {
            var y = Input.GetAxis("Mouse Y");
            _menuScrollAccumulator += y;
            if (Mathf.Abs(_menuScrollAccumulator) > _inputConfig.MenuScrollThreshold)
            {
                var dir = _menuScrollAccumulator > 0 ? -1 : 1;
                if (dir < 0) _disambiguationMenu.SelectPrevious();
                else _disambiguationMenu.SelectNext();
                _menuScrollAccumulator = 0f; 
            }
        }

        private void ClearHighlights()
        {
            foreach (var c in _currentCandidates) c.SetHighlight(false);
            _currentCandidates.Clear();
        }
        
        
        private void OnTimelineSelectionChanged(int start, int end)
        {
            if (Presenter.SelectedStartIndex != start) Presenter.SelectedStartIndex = start;
            if (Presenter.SelectedEndIndex != end) Presenter.SelectedEndIndex = end;
        }
        
        public void OnDialTick(int deltaTicks)
        {
            if (!IsVisible || _state != InteractionState.Roaming) return;
            _timeline?.Nudge(deltaTicks);
        }
    }
}
