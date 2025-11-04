using System;
using System.Collections.Generic;
using System.Linq;
using Artigio.MVVMToolkit.Core.Services.Localization;

namespace Artigio.MVVMToolkit.Core.Content.NewSchema
{
    public static class NewSchemaLanguageMapper
    {
        private class LanguageInfo : ILanguage
        {
            public string Name { get; set; }
            public string Tag { get; set; }
            public bool IsDefault { get; set; }
        }

        public static IList<ILanguage> MapLanguages(NewRoot root)
        {
            var langs = root?.Dictionaries?.Languages ?? new List<LanguageItem>();
            var defaultTag = root?.PresentationSettings?.DefaultLanguage?.Tag ?? langs.FirstOrDefault()?.Tag;

            return langs.Select(l => new LanguageInfo
            {
                Name = l.Name,
                Tag = l.Tag,
                IsDefault = string.Equals(l.Tag, defaultTag, StringComparison.OrdinalIgnoreCase)
            }).Cast<ILanguage>().ToList();
        }
    }
}