using System.Collections.Generic;
using Psh.MVPToolkit.Core.MVP.Base;
using Psh.MVPToolkit.Core.Navigation;
using Psh.MVPToolkit.Core.UI;
using Sources.Features.GlobeScreen.Model;
using Sources.Features.GlobeScreen.Presenter;
using Sources.Features.RightPopup.Model;
using Sources.Features.RightPopup.Presenter;
using Sources.Infrastructure;
using Sources.Presentation.Core.Types;
using Sources.Presentation.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

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
            public const string GlobeScreenMarkersClass = "globe-screen__markers";
        }
        
        // UI Elements
        private VisualElement _markersContainer;
        private MediaBackground _media;
        private Timeline _timeline;
        private VisualElement _rightPopup;
        private bool _popupActive;
        
        private const int ScrollStep = 1;

        // Active Markers
        private readonly List<GlobeMarkerElement> _activeMarkers = new();
        private const float EarthRadius = 4f;
        
        
        // Dependencies
        [Inject] protected override GlobePresenter Presenter { get; set; }
        [Inject] private RightPopupPresenter _rightPopupPresenter;
        [Inject] private INavigationFlowController<ViewType> _navigationController;
        [Inject] private EarthController _earthController;

        // View configuration
        public override ViewType GetViewType() => ViewType.Globe;
        protected override string ContainerName => "globe-screen";
        

        protected override void OnEnable()
        {
            base.OnEnable();
            SetupUIElements();
            RegisterEventHandlers();
            Container.dataSource = Presenter;
            _rightPopup.dataSource = _rightPopupPresenter;
            RebuildMarkers(); 

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEventHandlers();
            Container.dataSource = null;
            _rightPopup.dataSource = null;
            ClearMarkers();
        }
        
        private void SetupUIElements()
        {
            _media = Container.Q<MediaBackground>(UI.GlobeScreenBgName);
            _timeline = Container.Q<Timeline>();
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
            
            // Wykonaj update pozycji natychmiast, żeby nie mrugały w (0,0)
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
                _rightPopupPresenter.SetData(data.ToPopupData());
            
            _popupActive = !_popupActive;
            _rightPopup.EnableInClassList(UI.RightPopupVisibleClass, _popupActive);
            
        }

        private void UpdateMarkersPosition()
        {
            if (_activeMarkers.Count == 0 || _earthController == null || _earthController.Camera == null) return;
            
            // Pobieramy transformację Ziemi (zakładamy, że EarthController jest na obiekcie, który się kręci 
            // LUB ma referencję do pivota - w Twoim kodzie EarthController obraca `cameraPivot`, a Ziemia stoi w miejscu?
            // SPRAWDŹMY EarthController.cs z Chunk 2:
            // EarthController obraca `cameraPivot`. Kamera patrzy na pivota. Ziemia (model) prawdopodobnie jest w (0,0,0).
            // Jeśli kamera krąży wokół Ziemi, to Ziemia jest statyczna w WorldSpace (chyba że się kręci sama).
            // Zakładam scenariusz: Ziemia jest w (0,0,0), Kamera orbituje.
            
            // UWAGA: Jeśli w Twoim projekcie Ziemia się kręci, użyj transformacji Ziemi. 
            // Jeśli Kamera orbituje, używamy statycznej pozycji (0,0,0) i rotacji Identity dla punktów bazowych.
            
            // Ale: Lat/Lon definiuje punkt względem SFERY. Więc jeśli sfera jest w (0,0,0) i nie ma rotacji własnej,
            // to LocalToWorldMatrix to po prostu translacja/skala.
            
            // Przyjmijmy, że Ziemia jest w _earthController.transform.position (jeśli to pivot) lub w (0,0,0).
            Vector3 earthCenter = Vector3.zero; 
            Quaternion earthRotation = Quaternion.identity; // Jeśli model ziemi się nie kręci, tylko kamera
            
            // Jeśli jednak model Ziemi jest dzieckiem cameraPivot i się obraca razem z nim -> to by było dziwne.
            // Z kodu EarthController wynika: cameraPivot się obraca (yaw/pitch), kamera jest dzieckiem i patrzy na pivota.
            // Czyli Ziemia stoi w miejscu, a my kręcimy kamerą. OK.
            
            foreach (var marker in _activeMarkers)
            {
                // 1. Oblicz pozycję lokalną na sferze (Vector3)
                Vector3 localPos = LatLonToVector3(marker.Data.Latitude, marker.Data.Longitude, EarthRadius);
                
                // 2. Oblicz pozycję w świecie (Ziemia w 0,0,0, bez rotacji własnej)
                // Jeśli Ziemia ma offset lub skalę, trzeba tu uwzględnić matrix.
                Vector3 worldPos = earthCenter + (earthRotation * localPos);
            
                // 3. Rzutowanie na ekran
                Vector3 screenPos = _earthController.Camera.WorldToScreenPoint(worldPos);
            
                // 4. Sprawdzenie czy punkt jest przed kamerą (Z > 0) i czy nie jest zasłonięty przez Ziemię
                bool isVisible = screenPos.z > 0 && !IsOccluded(worldPos, _earthController.Camera.transform.position, earthCenter, EarthRadius);
            
                if (isVisible)
                {
                    // 5. Konwersja Screen -> Panel (UIToolkit)
                    // Uwaga: ScreenPos.y w Unity jest od dołu, w UI Toolkit od góry
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
            // Prosty test Occlusion dla sfery:
            // Sprawdzamy, czy wektor normalny w punkcie jest zwrócony w stronę kamery.
            // Normalna na sferze w (0,0,0) to po prostu znormalizowana pozycja punktu.
            
            Vector3 normal = (point - sphereCenter).normalized;
            Vector3 viewDir = (cameraPos - point).normalized;
            
            // Dot product > 0 oznacza, że "widzimy" ściankę (kąt < 90 stopni)
            // Dodajemy mały bias (np. -0.1f), żeby markery na krawędzi nie znikały zbyt wcześnie
            return Vector3.Dot(normal, viewDir) < -0.05f; 
        }
        
        private Vector3 LatLonToVector3(float lat, float lon, float radius)
        {
            // Konwersja Lat/Lon na Vector3 w Unity (Y-up)
            // Lat: -90 (S) do 90 (N) -> Pitch (X axis)
            // Lon: -180 (W) do 180 (E) -> Yaw (Y axis)
            
            // Wzory zależą od mapowania UV tekstury Ziemi.
            // Standardowo:
            // x = r * cos(lat) * cos(lon)
            // y = r * sin(lat)
            // z = r * cos(lat) * sin(lon)
            
            float latRad = lat * Mathf.Deg2Rad;
            float lonRad = -lon * Mathf.Deg2Rad; // Minus często potrzebny w Unity dla Lon
        
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