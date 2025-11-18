using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Psh.MVPToolkit.Core.Content.NewSchema
{
    public class NewRoot
    {
        [JsonProperty("presentationSettings")] public PresentationSettings PresentationSettings { get; set; }
        [JsonProperty("dictionaries")] public Dictionaries Dictionaries { get; set; }
        [JsonProperty("screens")] public List<Screen> Screens { get; set; }
    }

    public class PresentationSettings
    {
        [JsonProperty("presentationId")] public string PresentationId { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("defaultLanguage")] public DefaultLanguage DefaultLanguage { get; set; }
    }

    public class DefaultLanguage
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("value")] public int Value { get; set; }
        [JsonProperty("tag")] public string Tag { get; set; }
    }

    public class Dictionaries
    {
        [JsonProperty("languages")] public List<LanguageItem> Languages { get; set; }
    }

    public class LanguageItem
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("tag")] public string Tag { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }

    public class Screen
    {
        [JsonProperty("moduleId")] public string ModuleId { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("referenceId")] public string ReferenceId { get; set; }
        [JsonProperty("subModules")] public List<Module> SubModules { get; set; }
    }

    public class Module
    {
        [JsonProperty("moduleType")] public int ModuleType { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("isTranslated")] public bool? IsTranslated { get; set; }

        // Uwaga: JToken, żeby wspierać zarówno obiekt (mapę językową), jak i string/liczbę/bool
        [JsonProperty("text")] public JToken Text { get; set; }
        [JsonProperty("files")] public JToken Files { get; set; }

        [JsonProperty("number")] public int? Number { get; set; }

        [JsonProperty("moduleId")] public string ModuleId { get; set; }
        [JsonProperty("referenceId")] public string ReferenceId { get; set; }
        [JsonProperty("subModules")] public List<Module> SubModules { get; set; }
        [JsonProperty("list")] public List<Module> List { get; set; }

        // Dowolne dodatkowe pola (np. items, children, gallery...) — wykryjemy listy i skalary
        [JsonExtensionData] public IDictionary<string, JToken> ExtensionData { get; set; }
    }
}