using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sources.Presentation.UI.Converters
{
    public static class UIConverters
    {
        private static bool _registered;

        [Preserve]  
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void RegisterConverters()
        {
            if (_registered) return;
            _registered = true;

            // bool -> StyleEnum<DisplayStyle>
            // true => StyleKeyword.Null (return to USS), false => DisplayStyle.None 
            ConverterGroups.RegisterGlobalConverter(
                (ref bool value) => (StyleEnum<DisplayStyle>)(value ? StyleKeyword.Null : DisplayStyle.None)
            );

            // bool? -> StyleEnum<DisplayStyle> 
            ConverterGroups.RegisterGlobalConverter(
                (ref bool? value) =>
                    value.HasValue
                        ? (StyleEnum<DisplayStyle>)(value.Value ? StyleKeyword.Null : DisplayStyle.None)
                        : (StyleEnum<DisplayStyle>)StyleKeyword.Null
            );
        }
    }

}