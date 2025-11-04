using System.Collections.Generic;
using PrimeTween;
using UnityEngine.UIElements;

namespace Artigio.MVVMToolkit.Core.UI
{
    public static class VisualElementAnimator
    {
        
        
        private static readonly Dictionary<string, List<Tween>> ActiveTweens = new();

        public static void AnimateMove(VisualElement element, float y, float animationDuration, string id)
        {
            var tween = Tween.Custom(
                startValue: element.style.bottom.value.value,
                endValue: y,
                duration: animationDuration,
                onValueChange: x => element.style.bottom = x,
                ease: Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );

            AddTweenToDictionary(id, tween);
        }

        public static void AnimateFade(VisualElement element, float startValue, float endValue, float animationDuration, string id)
        {
            element.style.opacity = startValue;
            var tween = Tween.Custom(
                startValue: startValue,
                endValue: endValue,
                duration: animationDuration,
                onValueChange: x => element.style.opacity = x,
                ease: Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );

            AddTweenToDictionary(id, tween);
        }

        public static void StopAnimation(string animationId)
        {
            if (!ActiveTweens.TryGetValue(animationId, out var tweens)) 
                return;
            
            tweens.ForEach(tween => tween.Stop());
            ActiveTweens.Remove(animationId);
        }

        private static void AddTweenToDictionary(string id, Tween tween)
        {
            if (!ActiveTweens.ContainsKey(id))
            {
                ActiveTweens[id] = new List<Tween>();
            }
            ActiveTweens[id].Add(tween);
        }
    }
}