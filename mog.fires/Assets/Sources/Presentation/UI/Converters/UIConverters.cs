using UnityEditor;
using UnityEngine.UIElements;

namespace Sources.Presentation.UI.Converters
{
    public static class UIConverters
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void RegisterConverters()
        {
            ConverterGroups.RegisterGlobalConverter(
                (ref bool value) => (StyleEnum<DisplayStyle>)(value ? DisplayStyle.Flex: DisplayStyle.None));
        }
    }
}