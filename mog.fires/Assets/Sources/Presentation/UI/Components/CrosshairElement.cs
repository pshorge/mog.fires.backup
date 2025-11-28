// using System.Collections.Generic;
// using PrimeTween;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace Sources.Presentation.UI.Components
// {
//     [UxmlElement("CrosshairElement")]
//     public partial class CrosshairElement : VisualElement
//     {
//         private const string BaseClass = "crosshair";
//         private const string CrossVClass = "crosshair__line--v";
//         private const string CrossHClass = "crosshair__line--h";
//         private const string DiamondClass = "crosshair__diamond";
//         
//         // Diamonds ordered from inner (d1) to outer (d4)
//         private VisualElement _d1, _d2, _d3, _d4;
//         
//         // List of independent tweens to handle different cycle modes (Yoyo vs Restart)
//         private readonly List<Tween> _activeTweens = new();
//
//         public CrosshairElement()
//         {
//             AddToClassList(BaseClass);
//             pickingMode = PickingMode.Ignore;
//             
//             BuildStructure();
//             
//             // Manage animation lifecycle based on panel attachment
//             RegisterCallback<AttachToPanelEvent>(_ => StartAnimation());
//             RegisterCallback<DetachFromPanelEvent>(_ => StopAnimation());
//         }
//
//         private void BuildStructure()
//         {
//             // Create Crosshair lines
//             var vLine = new VisualElement(); 
//             vLine.AddToClassList(CrossVClass); 
//             Add(vLine);
//             
//             var hLine = new VisualElement(); 
//             hLine.AddToClassList(CrossHClass); 
//             Add(hLine);
//
//             // Create Diamonds
//             _d1 = CreateDiamond("diamond--1");
//             _d2 = CreateDiamond("diamond--2");
//             _d3 = CreateDiamond("diamond--3");
//             _d4 = CreateDiamond("diamond--4");
//         }
//
//         private VisualElement CreateDiamond(string subClass)
//         {
//             var diamond = new VisualElement();
//             diamond.AddToClassList(DiamondClass);
//             diamond.AddToClassList(subClass);
//             Add(diamond);
//             return diamond;
//         }
//
//         private void StartAnimation()
//         {
//             StopAnimation();
//
//             // ANIMATION STYLE: "Ethereal Breath"
//             // Crosshair lines and D1 are static anchors.
//             // D2, D3, D4 breathe slowly and synchronously.
//
//             float duration = 3.0f; // Slow, smooth tempo
//             Ease ease = Ease.InOutSine; // The smoothest non-linear easing
//
//             // D1: STATIC (No animation added)
//
//             // D2: Subtle breath
//             // Scale: 1.0 -> 1.04 (Very minimal)
//             // Opacity: 0.8 -> 0.5 (Never disappears)
//             _activeTweens.Add(Tween.Custom(1f, 1.04f, duration, v => SetScale(_d2, v), ease, cycles: -1, cycleMode: CycleMode.Yoyo));
//             _activeTweens.Add(Tween.Custom(0.8f, 0.5f, duration, v => _d2.style.opacity = v, ease, cycles: -1, cycleMode: CycleMode.Yoyo));
//             
//             // D3: Slightly wider breath
//             // Scale: 1.0 -> 1.07
//             // Opacity: 0.6 -> 0.3
//             _activeTweens.Add(Tween.Custom(1f, 1.07f, duration, v => SetScale(_d3, v), ease, cycles: -1, cycleMode: CycleMode.Yoyo));
//             _activeTweens.Add(Tween.Custom(0.6f, 0.3f, duration, v => _d3.style.opacity = v, ease, cycles: -1, cycleMode: CycleMode.Yoyo));
//             
//             // D4: Widest breath
//             // Scale: 1.0 -> 1.10
//             // Opacity: 0.5 -> 0.2
//             _activeTweens.Add(Tween.Custom(1f, 1.10f, duration, v => SetScale(_d4, v), ease, cycles: -1, cycleMode: CycleMode.Yoyo));
//             _activeTweens.Add(Tween.Custom(0.5f, 0.2f, duration, v => _d4.style.opacity = v, ease, cycles: -1, cycleMode: CycleMode.Yoyo));
//         }
//
//         private void SetScale(VisualElement element, float value)
//         {
//             element.style.scale = new Scale(Vector2.one * value);
//         }
//
//         private void StopAnimation()
//         {
//             foreach (var t in _activeTweens)
//             {
//                 if (t.isAlive) t.Stop();
//             }
//             _activeTweens.Clear();
//         }
//     }
// }

using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Components
{
    [UxmlElement("CrosshairElement")]
    public partial class CrosshairElement : VisualElement
    {
        private const string BaseClass = "crosshair";
        private const string VignetteClass = "crosshair__vignette";
        private const string WrapperClass = "crosshair__wrapper";
        private const string ShadowClass = "crosshair__shadow";
        private const string MainClass = "crosshair__main";
        
        private VisualElement _d1Wrapper, _d2Wrapper, _d3Wrapper, _d4Wrapper;
        private VisualElement _vignette;
        
        private readonly List<Tween> _activeTweens = new();

        public CrosshairElement()
        {
            AddToClassList(BaseClass);
            pickingMode = PickingMode.Ignore;
            
            BuildStructure();
            
            RegisterCallback<AttachToPanelEvent>(_ => StartAnimation());
            RegisterCallback<DetachFromPanelEvent>(_ => StopAnimation());
        }

        private void BuildStructure()
        {
            // 1. VIGNETTE (Procedural Background Glow)
            _vignette = new VisualElement();
            _vignette.AddToClassList(VignetteClass);
            _vignette.style.backgroundImage = GenerateVignetteTexture(); // Procedural generation
            Add(_vignette);

            // 2. CROSS LINES (Static)
            CreateLine("crosshair__line--v");
            CreateLine("crosshair__line--h");

            // 3. DIAMONDS (Animated Wrappers)
            _d1Wrapper = CreateDiamondGroup("diamond--1");
            _d2Wrapper = CreateDiamondGroup("diamond--2");
            _d3Wrapper = CreateDiamondGroup("diamond--3");
            _d4Wrapper = CreateDiamondGroup("diamond--4");
        }

        // Generates a simple radial gradient texture (Black Center -> Transparent Edge)
        // Or Black Edge -> Transparent Center? Usually vignettes are dark at edges, 
        // but for a "Glow" backing, we want Dark Center fading out.
        private StyleBackground GenerateVignetteTexture()
        {
            int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            var center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / maxDist);
                    
                    // Gradient: Center is Dark (Alpha 0.8), Edge is Transparent (Alpha 0)
                    // Using SmoothStep for softer falloff
                    float alpha = Mathf.SmoothStep(0.2f, 0f, t); 
                    
                    texture.SetPixel(x, y, new Color(80, 0, 0, alpha));
                }
            }
            texture.Apply();
            return new StyleBackground(texture);
        }

        private void CreateLine(string sizeClass)
        {
            var wrapper = new VisualElement();
            wrapper.pickingMode = PickingMode.Ignore;
            wrapper.style.position = Position.Absolute;
            wrapper.style.alignItems = Align.Center;
            wrapper.style.justifyContent = Justify.Center;
            
            var shadow = new VisualElement();
            shadow.AddToClassList(sizeClass);
            shadow.AddToClassList(ShadowClass);
            wrapper.Add(shadow);
            
            var main = new VisualElement();
            main.AddToClassList(sizeClass);
            main.AddToClassList(MainClass);
            wrapper.Add(main);
            
            Add(wrapper);
        }

        private VisualElement CreateDiamondGroup(string sizeClass)
        {
            var wrapper = new VisualElement();
            wrapper.AddToClassList(WrapperClass); 
            
            var shadow = new VisualElement();
            shadow.AddToClassList("crosshair__diamond");
            shadow.AddToClassList(ShadowClass);
            shadow.AddToClassList(sizeClass);
            wrapper.Add(shadow);

            var main = new VisualElement();
            main.AddToClassList("crosshair__diamond");
            main.AddToClassList(MainClass);
            main.AddToClassList(sizeClass);
            wrapper.Add(main);

            Add(wrapper);
            return wrapper;
        }

        private void StartAnimation()
        {
            StopAnimation();
            float duration = 3.0f; 
            Ease ease = Ease.InOutSine;

            // D1: Static
            
            // D2
            _activeTweens.Add(Tween.Custom(1f, 1/1.04f, duration, v => SetScale(_d2Wrapper, v), ease, cycles: -1, cycleMode: CycleMode.Yoyo));
            _activeTweens.Add(Tween.Custom(1f, 0.7f, duration, v => _d2Wrapper.style.opacity = v, ease, cycles: -1, cycleMode: CycleMode.Yoyo));
            
            // D3
            _activeTweens.Add(Tween.Custom(1f, 1/1.07f, duration, v => SetScale(_d3Wrapper, v), ease, cycles: -1, cycleMode: CycleMode.Yoyo));
            _activeTweens.Add(Tween.Custom(1f, 0.6f, duration, v => _d3Wrapper.style.opacity = v, ease, cycles: -1, cycleMode: CycleMode.Yoyo));
            
            // D4
            _activeTweens.Add(Tween.Custom(1f, 1/1.10f, duration, v => SetScale(_d4Wrapper, v), ease, cycles: -1, cycleMode: CycleMode.Yoyo));
            _activeTweens.Add(Tween.Custom(1f, 0.4f, duration, v => _d4Wrapper.style.opacity = v, ease, cycles: -1, cycleMode: CycleMode.Yoyo));
        }

        private void SetScale(VisualElement element, float value)
        {
            element.style.scale = new Scale(Vector2.one * value);
        }

        private void StopAnimation()
        {
            foreach (var t in _activeTweens) if (t.isAlive) t.Stop();
            _activeTweens.Clear();
        }
    }
}