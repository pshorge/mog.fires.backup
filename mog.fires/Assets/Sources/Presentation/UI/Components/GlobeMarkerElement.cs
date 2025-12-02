using System;
using Sources.Data.Models;
using Sources.Features.GlobeScreen.Model;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Components
{
    public class GlobeMarkerElement : VisualElement
    {
        private const string ClassName = "globe-marker";
        private const string HighlightClassName = "globe-marker--highlight";
        
        public PointData Data { get; private set; }


        public GlobeMarkerElement(PointData data)
        {
            Data = data;
            AddToClassList(ClassName);
            pickingMode = PickingMode.Position;
        }

        public void SetHighlight(bool state)
        {
            EnableInClassList(HighlightClassName, state);
            if(state)
                BringToFront();
        }
        

        public void Dispose()
        {
            Data = null;
        }
    }
}