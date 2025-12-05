using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Psh.MVPToolkit.Core.Content.NewSchema;
using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.Data.Models;
using UnityEngine;

namespace Sources.Data.Repositories
{
    public static class NewSchemaRepository
    {
        // Zwraca: root, ustawienia aplikacji (timeout, screensaver), listę języków oraz rejestr tłumaczeń
        public static (NewRoot root,
                       AppSettings settings,
                       IList<ILanguage> languages,
                       Dictionary<string, IDictionary<string, string>> registry)
            Load(string filePath, string targetScreenKey)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"{filePath} file does not exist!");
                    return default;
                }

                var json = File.ReadAllText(filePath);

                // Parsowanie i mapowanie
                var root = NewSchemaParser.Parse(json);
                var languages = NewSchemaLanguageMapper.MapLanguages(root);
                var registry = NewSchemaTraversal.BuildTranslationRegistry(root, targetScreenKey);

                // Wyznacz preferowany tag języka (domyślny → pierwszy)
                var preferredTag = languages.FirstOrDefault(l => l.IsDefault)?.Tag
                                   ?? languages.FirstOrDefault()?.Tag;

                // Wyciągnięcie ustawień na podstawie rejestru (klucze: "screensaver-timeout", "screensaver-file")
                var settings = ExtractAppSettingsFromRegistry(registry, preferredTag);

                return (root, settings, languages, registry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load JSON from path: {filePath}\n\n{ex.Message}\n{ex.StackTrace}");
                return default;
            }
        }

        // ————————————————————————————
        // Helpers
        // ————————————————————————————

        private static AppSettings ExtractAppSettingsFromRegistry(
            IReadOnlyDictionary<string, IDictionary<string, string>> registry,
            string preferredTag)
        {
            var settings = new AppSettings
            {
                ScreensaverTimeoutSeconds = 200,
                ScreensaverFile = null,
                ScreensaverEnabled = false
            };

            
            // timeout
            var timeoutStr = SelectByTag(registry, "screensaver-timeout", preferredTag);
            if (!string.IsNullOrWhiteSpace(timeoutStr))
            {
                if (int.TryParse(timeoutStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var t))
                    settings.ScreensaverTimeoutSeconds = t;
            }

            // screensaver file 
            var saver = SelectByTag(registry, "screensaver-file", preferredTag);
            if (!string.IsNullOrWhiteSpace(saver) && File.Exists(ContentPathResolver.ResolveContentPath(saver)))
            {
                settings.ScreensaverFile = saver;
                //can be enabled
                var enabledStr = SelectByTag(registry, "screensaver-enabled", preferredTag);
                if (!string.IsNullOrWhiteSpace(enabledStr))
                {
                    if (bool.TryParse(enabledStr, out var enabled))
                        settings.ScreensaverEnabled = enabled;
                }
            }
            
            return settings;
        }

        // Wybiera wartość według preferowanego języka, potem default, potem pierwszy niepusty
        private static string SelectByTag(
            IReadOnlyDictionary<string, IDictionary<string, string>> registry,
            string key,
            string preferredTag)
        {
            if (registry == null) return null;
            if (!registry.TryGetValue(key, out var map) || map == null || map.Count == 0) return null;

            if (!string.IsNullOrEmpty(preferredTag) &&
                map.TryGetValue(preferredTag, out var vPreferred) &&
                !string.IsNullOrWhiteSpace(vPreferred))
                return vPreferred;

            if (map.TryGetValue("default", out var vDefault) && !string.IsNullOrWhiteSpace(vDefault))
                return vDefault;

            return map.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}