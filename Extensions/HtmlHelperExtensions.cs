using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace TiengAnh.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent DisplayPartOfSpeech(this IHtmlHelper html, string code)
        {
            if (string.IsNullOrEmpty(code))
                return new HtmlString(string.Empty);

            Dictionary<string, string> types = new Dictionary<string, string>
            {
                { "n", "Danh từ (Noun)" },
                { "v", "Động từ (Verb)" },
                { "adj", "Tính từ (Adjective)" },
                { "adv", "Trạng từ (Adverb)" },
                { "prep", "Giới từ (Preposition)" },
                { "conj", "Liên từ (Conjunction)" },
                { "pron", "Đại từ (Pronoun)" },
                { "det", "Hạn định từ (Determiner)" },
                { "interj", "Thán từ (Interjection)" },
                { "noun", "Danh từ (Noun)" },
                { "verb", "Động từ (Verb)" }
            };
            
            return new HtmlString(types.ContainsKey(code.ToLower()) ? types[code.ToLower()] : code);
        }
    }
}
