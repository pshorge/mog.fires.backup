using System.Collections.Generic;
using Sources.Features.GlobeScreen.Model;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Components
{
    [UxmlElement("DisambiguationMenu")]
    public partial class DisambiguationMenu : VisualElement
    {
        private const string MenuClass = "disambiguation-menu";
        private const string VisibleClass = "disambiguation-menu--visible";
        private const string ItemClass = "disambiguation-item";
        private const string ItemSelectedClass = "disambiguation-item--selected";
        private const string LabelClass = "disambiguation-item__label";
        private const string SubLabelClass = "disambiguation-item__sublabel";

        private readonly List<GlobePointData> _currentData = new();
        private readonly List<VisualElement> _itemElements = new();
        private int _selectedIndex = -1;

        public bool IsMenuVisible => ClassListContains(VisibleClass);

        public DisambiguationMenu()
        {
            AddToClassList(MenuClass);
            pickingMode = PickingMode.Ignore;
        }

        public void Show(List<GlobePointData> items)
        {
            if (items == null || items.Count == 0)
            {
                Hide();
                return;
            }

            _currentData.Clear();
            _currentData.AddRange(items);
            
            RebuildVisuals();
            SetSelection(0);
            AddToClassList(VisibleClass);
        }

        public void Hide()
        {
            RemoveFromClassList(VisibleClass);
            Clear();
            _currentData.Clear();
            _itemElements.Clear();
            _selectedIndex = -1;
        }

        public void SelectNext() => SetSelection(_selectedIndex + 1);
        public void SelectPrevious() => SetSelection(_selectedIndex - 1);

        public GlobePointData GetSelectedItem()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _currentData.Count)
                return _currentData[_selectedIndex];
            return null;
        }

        private void SetSelection(int index)
        {
            if (_itemElements.Count == 0) return;

           
            if (index < 0) index = _itemElements.Count - 1;
            if (index >= _itemElements.Count) index = 0;

            // Update visuals
            if (_selectedIndex >= 0 && _selectedIndex < _itemElements.Count)
                _itemElements[_selectedIndex].RemoveFromClassList(ItemSelectedClass);

            _selectedIndex = index;
            
            if (_selectedIndex >= 0 && _selectedIndex < _itemElements.Count)
                _itemElements[_selectedIndex].AddToClassList(ItemSelectedClass);
        }

        private void RebuildVisuals()
        {
            Clear();
            _itemElements.Clear();

            foreach (var item in _currentData)
            {
                var row = new VisualElement();
                row.AddToClassList(ItemClass);

                var label = new Label(item.Place ?? "Unknown Place");
                label.AddToClassList(LabelClass);
                row.Add(label);

                if (!string.IsNullOrEmpty(item.Date))
                {
                    var subLabel = new Label(item.Date);
                    subLabel.AddToClassList(SubLabelClass);
                    row.Add(subLabel);
                }
                hierarchy.Add(row);
                //Add(row);
                _itemElements.Add(row);
            }
        }
    }
}