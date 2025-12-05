using System;
using System.Collections.Generic;
using System.Linq;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Data.Models;
using Sources.Features.GlobeScreen.Model;
using Sources.Features.GlobeScreen.Presenter;
using Sources.Features.Popup.Presenter;
using Sources.Infrastructure;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Presentation.Core.Types;
using Sources.Presentation.UI.Components;
using UnityEngine;
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
            public const string GlobeScreenLeftPopupName = "left_popup";
            public const string GlobeScreenRightPopupName = "right_popup";
            public const string LeftPopupVisibleClass = "left-popup--visible";
            public const string RightPopupVisibleClass = "right-popup--visible";
            public const string GlobeScreenMarkersClass = "globe-screen__markers";
            public const string GlobeScreenDisambiguationMenuName = "globe-screen__disambiguation_menu";
        }
        
        // UI Elements
        private VisualElement _markersContainer;
        private MediaBackground _media;
        private Timeline _timeline;
        private VisualElement _leftPopup;
        private VisualElement _rightPopup;
        private bool _popupActive;
        private DisambiguationMenu _disambiguationMenu;
        
        private const int ScrollStep = 1;

        // Active Markers
        private readonly List<GlobeMarkerElement> _activeMarkers = new();
        private const float EarthRadius = 4f;
        
        [Header("Interaction")]
        [SerializeField] private float selectionThresholdPixels = 100;
        [SerializeField] private float menuScrollThreshold = 20f;
        private float _menuScrollAccumulator = 0f;
        private List<GlobeMarkerElement> _currentCandidates = new();
        private InteractionState _state = InteractionState.Roaming;
        
        // Dependencies
        [Inject] protected override GlobePresenter Presenter { get; set; }
        [Inject] private PopupPresenter _popupPresenter;
        [Inject] private EarthController _earthController;
        [Inject] private IUnifiedInputService _inputService;

        // View configuration
        public override ViewType GetViewType() => ViewType.Globe;
        protected override string ContainerName => "globe-screen";
        
        
        [Header("Globe Calibration")]
        [SerializeField] private float lonOffset = -100f; // Initial calibration value ( texture offset problem probably)
        [SerializeField] private float latOffset;
        [SerializeField] private bool invertLon; 
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            Container.dataSource = Presenter;
            _leftPopup.dataSource = _popupPresenter;
            _rightPopup.dataSource = _popupPresenter;
            RebuildMarkers(); 

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null;
            _leftPopup.dataSource = null;
            _rightPopup.dataSource = null;
            ClearMarkers();
        }
        
        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(UI.GlobeScreenBgName);
            _timeline = Container.Q<Timeline>();
            _leftPopup = Container.Q<VisualElement>(UI.GlobeScreenLeftPopupName);
            _rightPopup = Container.Q<VisualElement>(UI.GlobeScreenRightPopupName);
            _disambiguationMenu = Container.Q<DisambiguationMenu>(UI.GlobeScreenDisambiguationMenuName);
            _markersContainer = Container.Q<VisualElement>(className: UI.GlobeScreenMarkersClass);
        }
        
        private void RegisterEventHandlers()
        {
            _inputService.OnAction += HandleInput;
            _timeline.SelectionChanged += OnTimelineSelectionChanged;
            Presenter.propertyChanged += OnPresenterPropertyChanged;
        }

        private void UnregisterEventHandlers()
        {
            _inputService.OnAction -= HandleInput;
            _timeline.SelectionChanged -= OnTimelineSelectionChanged;
            Presenter.propertyChanged -= OnPresenterPropertyChanged;
        }
        
        private void OnPresenterPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(Presenter.VisiblePoints))
            {
                RebuildMarkers();
            }
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
                    else if(_currentCandidates.Count == 1)
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
                var marker = new GlobeMarkerElement(pointData);
                _markersContainer.Add(marker);
                _activeMarkers.Add(marker);
            }
            // update asap to prevent blinking
            UpdateMarkersPosition();
        }

        private void ClearMarkers()
        {
            foreach (var marker in _activeMarkers)
            {
                marker.Dispose();
                marker.RemoveFromHierarchy();
            }
            _activeMarkers.Clear();
        }
        
        private void SetState(InteractionState newState)
        {
            _state = newState;
            Debug.Log($"[GlobeView] State changed to: {newState}");

            switch (newState)
            {
                case InteractionState.Roaming:
                    if(IsVisible) _earthController?.SetInputActive(true);
                    _disambiguationMenu.Hide();
                    TogglePopup(false); 
                    ClearMenuHighlights();
                    break;

                case InteractionState.Disambiguation:
                    _earthController?.SetInputActive(false);
                    TogglePopup(false);
                    _disambiguationMenu.Show(_currentCandidates.Select(m => m.Data).ToList());
                    break;

                case InteractionState.Details:
                    _earthController?.SetInputActive(false);
                    _disambiguationMenu.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }
        
        private void TogglePopup(bool show, PointData data = null)
        {
            var enablePopup = show && data is not null;
            if (enablePopup) _popupPresenter.SetData(data.ToPopupData());
            _leftPopup.EnableInClassList(UI.LeftPopupVisibleClass, enablePopup);
            _rightPopup.EnableInClassList(UI.RightPopupVisibleClass, enablePopup);
        }
        
        private void UpdateCandidateSelection()
        {
            if (_activeMarkers.Count == 0 || _earthController == null) return;

            Vector2 screenCenter = new (Screen.width / 2f, Screen.height / 2f);
            Vector3 earthCenter = Vector3.zero;
            //float closestDist = float.MaxValue;
            var newCandidates = new List<GlobeMarkerElement>();

            foreach (var marker in _activeMarkers)
            {
                var localPos = LatLonToVector3(marker.Data.Latitude, marker.Data.Longitude, EarthRadius);
                var worldPos = earthCenter + localPos; 
                
                if (IsOccluded(worldPos, _earthController.Camera.transform.position, earthCenter, EarthRadius))
                    continue;
                
                var screenPos3D = _earthController.Camera.WorldToScreenPoint(worldPos);
                if (screenPos3D.z <= 0) continue;

                var markerScreenPos = new Vector2(screenPos3D.x, screenPos3D.y);
                var dist = Vector2.Distance(markerScreenPos, screenCenter);
                if (dist <= selectionThresholdPixels) 
                    newCandidates.Add(marker);
            }
            
            // Sort candidates (center's closest)
            newCandidates.Sort((a, b) => 
            {
                var wa = earthCenter + LatLonToVector3(a.Data.Latitude, a.Data.Longitude, EarthRadius);
                var wb = earthCenter + LatLonToVector3(b.Data.Latitude, b.Data.Longitude, EarthRadius);
                var sa = _earthController.Camera.WorldToScreenPoint(wa);
                var sb = _earthController.Camera.WorldToScreenPoint(wb);
                return Vector2.Distance(sa, screenCenter).CompareTo(Vector2.Distance(sb, screenCenter));
            });
            
            bool changed = _currentCandidates.Count != newCandidates.Count;
            if (!changed && _currentCandidates.Count > 0 && newCandidates.Count > 0)
            {
                changed = _currentCandidates[0] != newCandidates[0];
            }

            if (changed)
            {
                ClearMenuHighlights();
                _currentCandidates = newCandidates;
                
                foreach (var c in _currentCandidates)
                    c.SetHighlight(true);
            }
        }
        
        private void ClearMenuHighlights()
        {
            foreach (var c in _currentCandidates) 
                c.SetHighlight(false);
            _currentCandidates.Clear();
        }
        
        private void UpdateMarkersPosition()
        {
            if (_activeMarkers.Count == 0 || _earthController == null || _earthController.Camera == null) return;
            
            // Determine Earth transformation.
            // EarthController logic: The Camera pivots around a point, while the Earth model likely stays static at (0,0,0).
            Vector3 earthCenter = Vector3.zero; 
            Quaternion earthRotation = Quaternion.identity; // If the Earth model doesn't rotate, only the camera orbits
            
            // If the Earth model itself rotates/moves, use its transform. 
            // Assuming here: Earth is static at world origin.
            
            foreach (var marker in _activeMarkers)
            {
                // 1. Calculate local position on the sphere (Vector3)
                Vector3 localPos = LatLonToVector3(marker.Data.Latitude, marker.Data.Longitude, EarthRadius);
                
                // 2. Calculate world position (Earth at 0,0,0, no self-rotation)
                // If Earth has offset/scale/rotation, apply matrix here.
                Vector3 worldPos = earthCenter + (earthRotation * localPos);
            
                //  3. Project to screen
                Vector3 screenPos = _earthController.Camera.WorldToScreenPoint(worldPos);
            
                // 4. Check if point is in front of camera (Z > 0) and not occluded by Earth
                bool isVisible = screenPos.z > 0 && !IsOccluded(worldPos, _earthController.Camera.transform.position, earthCenter, EarthRadius);
            
                if (isVisible)
                {
                    // 5. Conversion Screen -> Panel (UIToolkit)
                    // Note: Unity ScreenPos.y is bottom-up, UI Toolkit is top-down
                    screenPos.y = Screen.height - screenPos.y;
                    
                    Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(Container.panel, new Vector2(screenPos.x, screenPos.y));
            
                    marker.style.display = DisplayStyle.Flex;
                    marker.style.left = panelPos.x;
                    marker.style.top = panelPos.y;
                }
                else
                {
                    marker.style.display = DisplayStyle.None;
                }
            }
        }
        
        private bool IsOccluded(Vector3 point, Vector3 cameraPos, Vector3 sphereCenter, float radius)
        {
            // Simple Occlusion test for a sphere:
            // Check if normal vector at the point faces the camera.
            // Normal on a sphere at (0,0,0) is just the normalized position.
            
            Vector3 normal = (point - sphereCenter).normalized;
            Vector3 viewDir = (cameraPos - point).normalized;
            
            // Dot product > 0 means we "see" the face (angle < 90 degrees)
            // Add small bias (e.g. -0.05f) so markers on the edge don't disappear too early
            return Vector3.Dot(normal, viewDir) < -0.05f; 
        }
        
        private Vector3 LatLonToVector3(float lat, float lon, float radius)
        {
           
            // invert E <-> W
            float fixedLon = invertLon ? -lon : lon;
            fixedLon += lonOffset;
    
            float fixedLat = lat + latOffset;

            float latRad = fixedLat * Mathf.Deg2Rad;
            float lonRad = fixedLon * Mathf.Deg2Rad; 
            
            float x = radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
            float y = radius * Mathf.Sin(latRad);
            float z = radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);

            return new Vector3(x, y, z);
        }
        
        private void OnTimelineSelectionChanged(int start, int end)
        {
            if (Presenter.SelectedStartIndex != start) 
                Presenter.SelectedStartIndex = start;
            if (Presenter.SelectedEndIndex != end) 
                Presenter.SelectedEndIndex = end;
        }
        
        public void OnDialTick(int deltaTicks)
        {
            if (!IsVisible || _state != InteractionState.Roaming) return;
            _timeline?.Nudge(deltaTicks);
        }

        public override void Show()
        {
            base.Show();
            _media?.Play();
            SetState(InteractionState.Roaming);
        }
        
        public override void Hide()
        {
            base.Hide();
            _media?.Pause();
            _earthController?.SetInputActive(false);
            _disambiguationMenu.Hide();
            TogglePopup(false);
        }

        protected override void Update()
        {
            base.Update();
            UpdateMarkersPosition();
            
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
        
        private void HandleTimelineInput()
        {
            var scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) < 0.1f) return;
            
            int dir = scrollDelta < 0 ? 1 : -1;
            _timeline?.Nudge(dir * ScrollStep);
        }
        
        private void HandleMouseScrollMenu()
        {
            var y = Input.GetAxis("Mouse Y");
            _menuScrollAccumulator += y;
            if (Mathf.Abs(_menuScrollAccumulator) > menuScrollThreshold)
            {
                var dir = _menuScrollAccumulator > 0 ? -1 : 1;
                if (dir < 0) _disambiguationMenu.SelectPrevious();
                else _disambiguationMenu.SelectNext();
                _menuScrollAccumulator = 0f; 
            }
        }
    }
}