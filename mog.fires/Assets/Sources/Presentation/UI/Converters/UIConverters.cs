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
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void RegisterConverters()
        {
            if (_registered) return;
            _registered = true;

            // bool -> StyleEnum<DisplayStyle> (true => Flex, false => None)
            ConverterGroups.RegisterGlobalConverter(
                (ref bool value) => value ? DisplayStyle.Flex : DisplayStyle.None
            );

            // bool? -> StyleEnum<DisplayStyle>
            // null => StyleKeyword.Null (return control to USS), true => Flex, false => None
            ConverterGroups.RegisterGlobalConverter(
                (ref bool? value) =>
                    value.HasValue
                        ? (StyleEnum<DisplayStyle>)(value.Value ? DisplayStyle.Flex : DisplayStyle.None)
                        : (StyleEnum<DisplayStyle>)StyleKeyword.Null
            );
        }
    }

}