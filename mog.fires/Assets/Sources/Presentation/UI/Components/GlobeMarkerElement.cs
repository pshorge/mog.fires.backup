using System;
using Sources.Features.GlobeScreen.Model;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Components
{
    public class GlobeMarkerElement : VisualElement
    {
        private const string UssClassName = "globe-marker";
        
        public GlobePointData Data { get; private set; }

        public event Action<GlobePointData> OnMarkerClicked;

        public GlobeMarkerElement(GlobePointData data)
        {
            Data = data;
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Position;
            RegisterCallback<ClickEvent>(OnClicked);
        }

        private void OnClicked(ClickEvent evt)
        {
            OnMarkerClicked?.Invoke(Data);
            evt.StopPropagation(); 
        }

        public void Dispose()
        {
            UnregisterCallback<ClickEvent>(OnClicked);
            OnMarkerClicked = null;
            Data = null;
        }
    }
}