using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Artigio.MVVMToolkit.Core.Services.Localization
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LocalizationService : ILocalizationService
    {
        public event Action LanguageChanged;

        private readonly List<ILanguage> _languages;
        private int _currentLangIndex;

        private readonly Dictionary<string, Dictionary<string, string>> _registry = new(StringComparer.OrdinalIgnoreCase);

        public LocalizationService(IEnumerable<ILanguage> langs)
        {
            _languages = langs?.ToList() ?? new List<ILanguage>();
            SetDefaultLanguage();
        }

        public void SetLanguage(int languageIndex)
        {
            if (_languages is null || languageIndex < 0 || languageIndex >= _languages.Count) return;
            _currentLangIndex = languageIndex;
            LanguageChanged?.Invoke();
        }

        public void SetLanguage(string languageTag)
        {
            if (_languages is null) return;
            var index = _languages.FindIndex(l => l.Tag == languageTag);
            if (index >= 0) SetLanguage(index);
        }

        public void ChangeLanguage()
        {
            if(_languages is not null && _languages.Count > 0)
                SetLanguage((_currentLangIndex + 1) % _languages.Count);
        }

        public void SetDefaultLanguage()
        {
            if(_languages is null || _languages.Count == 0) return;
            var index = _languages.FindIndex(l => l.IsDefault);
            if (index < 0) index = 0;
            SetLanguage(index);
        }

        public string GetLanguageTag()
        {
            var lang = _languages?.ElementAtOrDefault(_currentLangIndex);
            return lang?.Tag;
        }

        public string GetNextLanguageTag()
        {
            if(_languages is null ) return null;
            if (_languages.Count <= 1) return GetLanguageTag();
            var index = (_currentLangIndex + 1) % _languages.Count;
            return _languages.ElementAtOrDefault(index)?.Tag;
        }

        public void AddTranslation(string id, IDictionary<string, string> translations, bool overwrite = true)
        {
            // if (string.IsNullOrWhiteSpace(id) || translations == null) return;
            // if (!overwrite && _registry.ContainsKey(id)) return;
            // _registry[id] = new Dictionary<string, string>(translations, StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(id) || translations == null)
            {
                Debug.LogWarning($"Attempted to add translation with invalid input. ID: '{id}'");
                return;
            }
            bool keyExists = _registry.ContainsKey(id);
            if (keyExists && !overwrite)
            {
                Debug.Log($"Skipping addition for existing ID '{id}' as overwrite is disabled.");
                return;
            }
            string action = keyExists ? "Updating" : "Adding";
            Debug.Log($"{action} {translations.Count} translations for ID '{id}'.");
            _registry[id] = new Dictionary<string, string>(translations, StringComparer.OrdinalIgnoreCase);
        }

        public void AddTranslations(IEnumerable<KeyValuePair<string, IDictionary<string, string>>> entries, bool overwrite = true)
        {
            if (entries == null) return;
            foreach (var e in entries) 
                AddTranslation(e.Key, e.Value, overwrite);
        }

        public bool RemoveTranslation(string id) => _registry.Remove(id);

        public string GetTranslation(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (!_registry.TryGetValue(id, out var map) || map == null || map.Count == 0) return null;

            var tag = GetLanguageTag();
            if (tag != null && map.TryGetValue(tag, out var v) && !string.IsNullOrEmpty(v)) return v;

            var def = _languages?.FirstOrDefault(l => l.IsDefault)?.Tag;
            if (def != null && map.TryGetValue(def, out var dv) && !string.IsNullOrEmpty(dv)) return dv;

            return map.Values.FirstOrDefault(s => !string.IsNullOrEmpty(s));
        }

        public bool TryGetTranslation(string id, out string value)
        {
            value = GetTranslation(id);
            return value != null;
        }
    }
}