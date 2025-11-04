using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Artigio.MVVMToolkit.Core.Text
{
    public static class TextFormattingExtensions
    {
        private static readonly HashSet<string> AcceptableTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "align", "alpha", "color", "b", "i", "cspace", "font", "indent",
            "line-height", "line-indent", "link", "lowercase", "uppercase",
            "smallcaps", "margin", "mark", "mspace", "noparse", "nobr",
            "page", "pos", "size", "space", "s", "u", "sub", "sup", "offset", "width"
        };

        public static string HtmlToRichText(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string text = WebUtility.HtmlDecode(value);

            // Zamiana specyficznych tagów HTML na odpowiedniki Rich Text
            text = ReplaceHtmlTags(text);
            
            // Usuwanie nieobsługiwanych tagów i zabezpieczanie znaków specjalnych
            text = ProcessRemainingTags(text);
            text = EscapeStrayCharacters(text);

            return text.Trim();
        }

        private static string ReplaceHtmlTags(string text)
        {
            // Zamiana tagów strong na b
            text = Regex.Replace(text, @"<\s*strong\b[^>]*>", "<b>", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</\s*strong\s*>", "</b>", RegexOptions.IgnoreCase);

            // Zamiana nagłówków na rozmiary czcionki
            text = Regex.Replace(text, @"<\s*h1\b[^>]*>", "<size=150%>", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<\s*h2\b[^>]*>", "<size=120%>", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<\s*h3\b[^>]*>", "<size=110%>", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</\s*h[1-3]\s*>", "</size>", RegexOptions.IgnoreCase);

            // Zamiana <br> na znak nowej linii
            text = Regex.Replace(text, @"<\s*br\s*/?\s*>\s*", "\n", RegexOptions.IgnoreCase);

            return text;
        }

        private static string ProcessRemainingTags(string text)
        {
            return Regex.Replace(text, @"</?\s*([a-zA-Z]+)(\s+[^>]*)?>", match =>
            {
                var tagName = match.Groups[1].Value.ToLower();
                return AcceptableTags.Contains(tagName) ? match.Value : "";
            }, RegexOptions.IgnoreCase);
        }

        private static string EscapeStrayCharacters(string text)
        {
            return Regex.Replace(text, @"(<[^>]*>)|[<>]", match =>
            {
                if (match.Groups[1].Success) return match.Value;
                return match.Value == "<" ? "&lt;" : "&gt;";
            });
        }
    }
}