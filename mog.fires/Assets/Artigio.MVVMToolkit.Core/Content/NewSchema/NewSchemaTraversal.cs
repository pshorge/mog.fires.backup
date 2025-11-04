using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Artigio.MVVMToolkit.Core.Text;
using Newtonsoft.Json.Linq;

namespace Artigio.MVVMToolkit.Core.Content.NewSchema
{
    public static class NewSchemaTraversal
    {
        // Buduje rejestr tłumaczeń: klucz → mapa językowa lub { "default": ... } dla skalarów
        public static Dictionary<string, IDictionary<string, string>> BuildTranslationRegistry(NewRoot root, string targetScreenKey)
        {
            var bag = new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var screen = FindScreen(root, targetScreenKey);
            if (screen == null) return bag;

            var listStack = new List<ListSeg>();
            Walk(screen.SubModules, bag, listStack);
            return bag;
        }

        private static Screen FindScreen(NewRoot root, string key) =>
            (root?.Screens ?? new List<Screen>())
                .FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase))
            ?? root?.Screens?.FirstOrDefault();

        private struct ListSeg
        {
            public string Name;
            public int Index; // 0-based
            public ListSeg(string name, int index) { Name = name; Index = index; }
        }

        private static void Walk(IEnumerable<Module> modules, Dictionary<string, IDictionary<string, string>> bag, List<ListSeg> listStack)
        {
            if (modules == null) return;

            foreach (var m in modules)
            {
                var id = BuildKey(listStack, m.Key);

                // TEXT: mapa językowa (object) lub skalar (string/liczba/bool) jako default
                if (m.Text != null && m.Text.Type != JTokenType.Null)
                {
                    if (m.Text.Type == JTokenType.Object)
                    {
                        var obj = (JObject)m.Text;
                        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var p in obj.Properties())
                            map[p.Name] = p.Value?.ToString()?.HtmlToRichText() ?? string.Empty;
                        Put(bag, id, map);
                    }
                    else if (m.Text is JValue tv)
                    {
                        Put(bag, id, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["default"] = ToInvariantString(tv)?.HtmlToRichText()
                        });
                    }
                }

                // FILES: mapa językowa (object) lub string jako default
                if (m.Files != null && m.Files.Type != JTokenType.Null)
                {
                    if (m.Files.Type == JTokenType.Object)
                    {
                        var obj = (JObject)m.Files;
                        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var p in obj.Properties())
                            map[p.Name] = p.Value?.ToString();
                        Put(bag, id, map);
                    }
                    else if (m.Files is JValue fv && fv.Type != JTokenType.Null)
                    {
                        Put(bag, id, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["default"] = ToInvariantString(fv)
                        });
                    }
                }

                // NUMBER: liczba → default (jeśli wcześniej nie zapisano innej wartości)
                if (m.Number.HasValue && !bag.ContainsKey(id))
                {
                    Put(bag, id, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["default"] = m.Number.Value.ToString(CultureInfo.InvariantCulture)
                    });
                }

                // Fallback: pierwszy sensowny skalar z ExtensionData (gdy nic nie trafia pod id)
                if (!bag.ContainsKey(id) && TryFindAnyScalar(m, out var scalar))
                {
                    Put(bag, id, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["default"] = scalar
                    });
                }

                // Rekurencja: zwykłe subModules (bez indeksów)
                if (m.SubModules != null && m.SubModules.Count > 0)
                    Walk(m.SubModules, bag, listStack);

                // Rekurencja: dowolne listy modułów (list/items/children/...)
                foreach (var list in EnumerateAnyModuleLists(m))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        listStack.Add(new ListSeg(m.Key, i));
                        Walk(new[] { item }, bag, listStack);
                        listStack.RemoveAt(listStack.Count - 1);
                    }
                }
            }
        }

        private static IEnumerable<List<Module>> EnumerateAnyModuleLists(Module m)
        {
            if (m.List != null && m.List.Count > 0)
                yield return m.List;

            if (m.ExtensionData == null) yield break;

            foreach (var kv in m.ExtensionData)
            {
                var prop = kv.Key;
                var token = kv.Value;

                if (string.Equals(prop, "subModules", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(prop, "selectedModules", StringComparison.OrdinalIgnoreCase)) continue;

                if (token is not JArray arr || arr.Count == 0) continue;
                if (!LooksLikeModuleArray(arr)) continue;

                List<Module> list;
                try { list = arr.ToObject<List<Module>>(); }
                catch { continue; }

                if (list != null && list.Count > 0)
                    yield return list;
            }
        }

        private static bool LooksLikeModuleArray(JArray arr)
        {
            var firstObj = arr.First as JObject;
            if (firstObj == null) return false;

            if (firstObj.Property("key") != null) return true;
            if (firstObj.Property("text") != null) return true;
            if (firstObj.Property("files") != null) return true;
            if (firstObj.Property("isTranslated") != null) return true;
            if (firstObj.Property("subModules") != null) return true;
            if (firstObj.Property("list") != null) return true;
            if (firstObj.Property("items") != null) return true;

            return false;
        }

        private static string BuildKey(List<ListSeg> stack, string leafKey)
        {
            if (stack == null || stack.Count == 0) return leafKey ?? string.Empty;

            var parts = new List<string>(stack.Count + 1);
            foreach (var seg in stack)
                if (!string.IsNullOrEmpty(seg.Name))
                    parts.Add($"{seg.Name}-{seg.Index + 1}");

            if (!string.IsNullOrEmpty(leafKey))
                parts.Add(leafKey);

            return string.Join("-", parts);
        }

        private static bool TryFindAnyScalar(Module m, out string scalar)
        {
            scalar = null;

            if (m.Number.HasValue)
            {
                scalar = m.Number.Value.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            if (m.ExtensionData != null)
            {
                var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "moduleType","key","name","moduleId","referenceId",
                    "isTranslated","text","files","number","subModules",
                    "list","selectedModules"
                };

                foreach (var kv in m.ExtensionData)
                {
                    if (skip.Contains(kv.Key)) continue;
                    if (kv.Value is JValue v && v.Type != JTokenType.Null)
                    {
                        var s = v.Value switch
                        {
                            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                            null => null,
                            _ => v.ToString()
                        };
                        if (!string.IsNullOrEmpty(s))
                        {
                            scalar = s;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static string ToInvariantString(JValue v)
        {
            return v?.Value switch
            {
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                null => null,
                _ => v.ToString()
            };
        }

        private static void Put(Dictionary<string, IDictionary<string, string>> bag, string key, IDictionary<string, string> map)
        {
            if (bag.ContainsKey(key))
            {
                // opcjonalnie: ostrzegaj o kolizji
                // UnityEngine.Debug.LogWarning($"[Localization] Duplicate key: {key} — overwritten");
            }
            bag[key] = map;
        }
    }
}