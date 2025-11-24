using System.Collections.Generic;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Features.GlobeScreen.Model;
using Sources.Features.GlobeScreen.Presenter;
using Sources.Features.Popup.Presenter;
using Sources.Infrastructure;
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
        }
        
        // UI Elements
        private VisualElement _markersContainer;
        private MediaBackground _media;
        private Timeline _timeline;
        private VisualElement _leftPopup;
        private VisualElement _rightPopup;
        private bool _popupActive;
        
        private const int ScrollStep = 1;

        // Active Markers
        private readonly List<GlobeMarkerElement> _activeMarkers = new();
        private const float EarthRadius = 4f;
        
        
        // Dependencies
        [Inject] protected override GlobePresenter Presenter { get; set; }
        [Inject] private PopupPresenter _popupPresenter;
        [Inject] private INavigationFlowController<ViewType> _navigationController;
        [Inject] private EarthController _earthController;

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
            _markersContainer = Container.Q<VisualElement>(className: UI.GlobeScreenMarkersClass);
        }

        private void RegisterEventHandlers()
        {
            Container.RegisterCallback<WheelEvent>(OnGlobalWheel, TrickleDown.TrickleDown);
            _timeline.SelectionChanged += OnTimelineSelectionChanged;
            Presenter.propertyChanged += OnPresenterPropertyChanged;
        }

        private void UnregisterEventHandlers()
        {
            Container.UnregisterCallback<WheelEvent>(OnGlobalWheel, TrickleDown.TrickleDown);
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

        
        
        private void RebuildMarkers()
        {
            ClearMarkers();
            
            if (Presenter.VisiblePoints == null) return;
            
            foreach (var pointData in Presenter.VisiblePoints)
            {
                var marker = new GlobeMarkerElement(pointData);
                marker.OnMarkerClicked += OnMarkerClicked;
                
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
                marker.OnMarkerClicked -= OnMarkerClicked;
                marker.RemoveFromHierarchy();
            }
            _activeMarkers.Clear();
        }

        private void OnMarkerClicked(GlobePointData data)
        {
            //inject data to popup if popup is inactive
            if(!_popupActive)
                _popupPresenter.SetData(data.ToPopupData());
            
            _popupActive = !_popupActive;
            
            _leftPopup.EnableInClassList(UI.LeftPopupVisibleClass, _popupActive);
            _rightPopup.EnableInClassList(UI.RightPopupVisibleClass, _popupActive);
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

        protected override void Update()
        {
            base.Update();
            if(IsVisible)
                UpdateMarkersPosition();
                
        }
    }
}