using System;
using System.Collections.Generic;

namespace Psh.MVPToolkit.Core.Services.Localization
{
    public interface ILocalizationService
    {
        event Action LanguageChanged;

        void SetLanguage(int languageIndex);
        void SetLanguage(string languageTag);
        void ChangeLanguage();
        void SetDefaultLanguage();

        string GetLanguageTag();
        string GetNextLanguageTag();

        void AddTranslation(string id, IDictionary<string, string> translations, bool overwrite = true);
        void AddTranslations(IEnumerable<KeyValuePair<string, IDictionary<string, string>>> entries, bool overwrite = true);
        bool RemoveTranslation(string id);
        string GetTranslation(string id);
        bool TryGetTranslation(string id, out string value);
    }
}
