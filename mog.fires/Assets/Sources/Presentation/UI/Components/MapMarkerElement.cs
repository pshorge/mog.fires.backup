using Sources.Data.Models;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Components
{
    public class MapMarkerElement : VisualElement
    {
        private const string ClassName = "map-marker";
        private const string HighlightClassName = "map-marker--highlight";
        
        public PointData Data { get; private set; }


        public MapMarkerElement(PointData data)
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