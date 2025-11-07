using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Components
{
    [UxmlElement("Timeline")]
    public partial class Timeline : VisualElement
    {
        private List<string> _items = new();
        [CreateProperty]
        public List<string> Items
        {
            get => _items;
            set { _items = value ?? new List<string>(); Rebuild(); }
        }

        [UxmlAttribute("line-thickness")] public float lineThickness { get; set; } = 1f;
        [UxmlAttribute("label-offset")]   public float labelOffset   { get; set; } = 24f;

        [UxmlAttribute("stem-height")] public float stemHeight { get; set; } = 24f;
        [UxmlAttribute("icon-gap")]    public float iconGap    { get; set; } = 0f;
        [UxmlAttribute("tick-icon-class")]
        public string TickIconClass { get; set; } = "timeline__tick-icon";

        private int _selectedStart = -1;
        [CreateProperty, UxmlAttribute("selected-start")]
        public int SelectedStart
        {
            get => _selectedStart;
            set { if (_selectedStart == value) return; _selectedStart = value; UpdateSelection(); }
        }

        private int _selectedEnd = -1;
        [CreateProperty, UxmlAttribute("selected-end")]
        public int SelectedEnd
        {
            get => _selectedEnd;
            set { if (_selectedEnd == value) return; _selectedEnd = value; UpdateSelection(); }
        }

        [UxmlAttribute("selection-height")] public float selectionHeight { get; set; } = 34f;
        [UxmlAttribute("selection-gap")]    public float selectionGap    { get; set; } = 14f;

        [UxmlAttribute("left-diamond-size")]  public float leftDiamondSize  { get; set; } = 14f;
        [UxmlAttribute("right-diamond-size")] public float rightDiamondSize { get; set; } = 12f;

        [UxmlAttribute("animated")] public bool animated { get; set; } = true;

        private const string DefaultFullClass = "timeline__selection--full";
        private string _fullClass;
        [CreateProperty, UxmlAttribute("selection-full")]
        public bool SelectionFull
        {
            get => _selectionFull;
            set
            {
                if (_selectionFull == value) return;
                _selectionFull = value;
                ApplyFullClass();
                UpdateSelection();
            }
        }
        private bool _selectionFull;

        [UxmlAttribute("full-class")]
        public string FullClass
        {
            get => _fullClass ?? DefaultFullClass;
            set { _fullClass = string.IsNullOrEmpty(value) ? DefaultFullClass : value; ApplyFullClass(); }
        }

        public event Action<int, int> SelectionChanged;

        private const string LabelSelectedClass = "timeline__label--selected";

        private readonly VisualElement _line;
        private readonly VisualElement _selection;
        private readonly VisualElement _selDiamondLeft;
        private readonly VisualElement _selDiamondRight;
        private readonly List<VisualElement> _ticks  = new();
        private readonly List<VisualElement> _stems  = new();
        private readonly List<VisualElement> _icons  = new();
        private readonly List<Label>         _labels = new();

        public Timeline()
        {
            AddToClassList("timeline");
            if (animated) AddToClassList("timeline--animated");
            pickingMode = PickingMode.Ignore;

            _line = new VisualElement { name = "line" };
            _line.AddToClassList("timeline__line");
            hierarchy.Add(_line);

            _selection = new VisualElement { name = "selection" };
            _selection.AddToClassList("timeline__selection");
            hierarchy.Add(_selection);

            _selDiamondLeft = new VisualElement { name = "selection-diamond-left" };
            _selDiamondLeft.AddToClassList("timeline__selection-diamond");
            _selDiamondLeft.AddToClassList("timeline__selection-diamond--left");
            _selection.hierarchy.Add(_selDiamondLeft);

            _selDiamondRight = new VisualElement { name = "selection-diamond-right" };
            _selDiamondRight.AddToClassList("timeline__selection-diamond");
            _selDiamondRight.AddToClassList("timeline__selection-diamond--right");
            _selection.hierarchy.Add(_selDiamondRight);

            RegisterCallback<AttachToPanelEvent>(_ => LayoutAll());
            RegisterCallback<GeometryChangedEvent>(_ => LayoutAll());
        }

        public void Nudge(int delta, bool raiseEvent = true)
        {
            if (delta == 0) return;
            int n = _items?.Count ?? 0;
            if (n < 2) return;

            int width = Mathf.Max(1, (_selectedEnd >= 0 ? _selectedEnd : 1) - (_selectedStart >= 0 ? _selectedStart : 0));
            int s = Mathf.Clamp((_selectedStart >= 0 ? _selectedStart : 0) + delta, 0, Math.Max(0, n - width));
            int e = s + width;
            SetSelectionInternal(s, e, raiseEvent);
        }
        public void SetSelection(int start, int end, bool raiseEvent = true) => SetSelectionInternal(start, end, raiseEvent);

        private void Rebuild()
        {
            foreach (var t in _ticks) t.RemoveFromHierarchy();
            foreach (var l in _labels) l.RemoveFromHierarchy();
            _ticks.Clear(); _stems.Clear(); _icons.Clear(); _labels.Clear();

            int n = _items?.Count ?? 0;
            if (n <= 0)
            {
                HideSelection();
                return;
            }

            for (int i = 0; i <= n; i++)
            {
                var tick = new VisualElement { name = $"tick-{i}" };
                tick.AddToClassList("timeline__tick");

                var stem = new VisualElement { name = $"tick-stem-{i}" };
                stem.AddToClassList("timeline__tick-stem");

                var icon = new VisualElement { name = $"tick-icon-{i}" };
                icon.AddToClassList("timeline__tick-icon");
                if (!string.IsNullOrEmpty(TickIconClass) && TickIconClass != "timeline__tick-icon")
                    icon.AddToClassList(TickIconClass);

                tick.hierarchy.Add(stem);
                tick.hierarchy.Add(icon);
                hierarchy.Add(tick);

                _ticks.Add(tick);
                _stems.Add(stem);
                _icons.Add(icon);
            }

            for (int i = 0; i < n; i++)
            {
                var label = new Label(_items[i]) { name = $"label-{i}" };
                label.AddToClassList("timeline__label");
                hierarchy.Add(label);
                _labels.Add(label);
            }

            ClampSelectionToItems();
            LayoutAll();
            ApplyFullClass();
        }

        private void LayoutAll()
        {
            float w = resolvedStyle.width;
            float h = resolvedStyle.height;
            if (w <= 1 || h <= 1) return;

            float lineY = h * 0.5f;

            _line.style.left = 0;
            _line.style.right = 0;
            _line.style.width = Length.Percent(100);
            _line.style.height = lineThickness;
            _line.style.top = lineY - lineThickness * 0.5f;

            int n = _items?.Count ?? 0;
            if (n <= 0)
            {
                HideSelection();
                return;
            }

            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                float x = Mathf.Lerp(0, w, t);

                var tick = _ticks[i];
                tick.style.position = Position.Absolute;
                tick.style.left = x;
                tick.style.top = lineY;
                tick.style.translate = new Translate(Length.Percent(-50), 0);

                var stem = _stems[i];
                stem.style.position = Position.Absolute;
                stem.style.left = 0;
                stem.style.top = 0;
                stem.style.width = lineThickness;
                stem.style.height = stemHeight;
                stem.style.translate = new Translate(Length.Percent(-50), 0);

                var icon = _icons[i];
                icon.style.position = Position.Absolute;
                icon.style.left = 0;
                icon.style.top = stemHeight + iconGap;
                icon.style.translate = new Translate(Length.Percent(-50), 0);
            }

            for (int i = 0; i < n; i++)
            {
                float tMid = (i + 0.5f) / n;
                float xMid = Mathf.Lerp(0, w, tMid);

                var label = _labels[i];
                label.text = _items[i];
                label.style.position = Position.Absolute;
                label.style.left = xMid;
                label.style.top = lineY + labelOffset;
                label.style.translate = new Translate(Length.Percent(-50), 0);
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            int n = _items?.Count ?? 0;
            float w = resolvedStyle.width;
            float h = resolvedStyle.height;

            if (n <= 0 || w <= 1 || h <= 1) { HideSelection(); return; }
            if (_selectedStart < 0 && _selectedEnd < 0) { HideSelection(); return; }

            int s = Mathf.Clamp(_selectedStart, 0, Mathf.Max(0, n - 1));
            int e = Mathf.Clamp(_selectedEnd,   1, n);
            if (e <= s) e = Mathf.Min(n, s + 1);

            float xStart = Mathf.Lerp(0, w, s / (float)n);
            float xEnd   = Mathf.Lerp(0, w, e / (float)n);

            if (SelectionFull)
            {
                xStart = 0f;
                xEnd   = w;
            }

            float lineY = h * 0.5f;
            float top   = lineY - selectionGap - selectionHeight;

            _selection.style.display  = DisplayStyle.Flex;
            _selection.style.position = Position.Absolute;
            _selection.style.left     = xStart;
            _selection.style.top      = top;
            _selection.style.width    = xEnd - xStart;
            _selection.style.height   = selectionHeight;

            _selDiamondLeft.style.width     = leftDiamondSize;
            _selDiamondLeft.style.height    = leftDiamondSize;
            _selDiamondLeft.style.left      = 0;
            _selDiamondLeft.style.top       = 0;
            _selDiamondLeft.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));

            _selDiamondRight.style.width     = rightDiamondSize;
            _selDiamondRight.style.height    = rightDiamondSize;
            _selDiamondRight.style.left      = Length.Percent(100);
            _selDiamondRight.style.top       = 0;
            _selDiamondRight.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));

           
            UpdateLabelHighlight(s, e, SelectionFull);
        }

        private void UpdateLabelHighlight(int s, int e, bool full)
        {
            int n = _labels.Count;
            for (int i = 0; i < n; i++)
            {
                bool selected = full ? (i == n - 1) : (i >= s && i < e);
                _labels[i].EnableInClassList(LabelSelectedClass, selected);
            }
        }

        private void HideSelection()
        {
            _selection.style.display = DisplayStyle.None;
            foreach (var lbl in _labels)
                lbl.EnableInClassList(LabelSelectedClass, false);
        }

        private void SetSelectionInternal(int s, int e, bool raiseEvent)
        {
            int n = _items?.Count ?? 0;
            if (n < 2)
            {
                _selectedStart = _selectedEnd = -1;
                HideSelection();
                return;
            }

            s = Mathf.Clamp(s, 0, Mathf.Max(0, n - 1));
            e = Mathf.Clamp(e, 1, n);
            if (e <= s) e = Mathf.Min(n, s + 1);

            _selectedStart = s;
            _selectedEnd   = e;
            UpdateSelection();

            if (raiseEvent)
                SelectionChanged?.Invoke(s, e);
        }

        private void ClampSelectionToItems()
        {
            int n = _items?.Count ?? 0;
            if (n < 2) { _selectedStart = _selectedEnd = -1; return; }

            int s = _selectedStart < 0 ? 0 : Mathf.Clamp(_selectedStart, 0, Math.Max(0, n - 1));
            int e = _selectedEnd   < 0 ? s + 1 : Mathf.Clamp(_selectedEnd,   1, n);
            if (e <= s) e = Mathf.Min(n, s + 1);

            _selectedStart = s;
            _selectedEnd   = e;
        }

        private void ApplyFullClass()
        {
            if (_selection == null) return;
            _selection.EnableInClassList(FullClass, _selectionFull);
        }
    }
}