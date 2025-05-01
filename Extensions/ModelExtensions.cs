using TiengAnh.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TiengAnh.Extensions
{
    public static class ModelExtensions
    {
        // Extension method để kiểm tra xem answer có đúng với option không
        public static bool IsCorrectAnswer(this ExerciseModel exercise, string option)
        {
            if (string.IsNullOrEmpty(exercise.Answer_BT))
                return false;
                
            return exercise.Answer_BT == option;
        }
        
        // Phương thức này chuyển đổi char thành string
        public static string ToOptionString(this char c)
        {
            return c.ToString();
        }
        
        // Extension method để render câu hỏi FillBlank
        public static IHtmlContent RenderFillBlankQuestion(this IHtmlHelper html, string question)
        {
            if (string.IsNullOrEmpty(question))
                return new HtmlString("");
                
            // Thay thế dấu ___ với input
            var processedQuestion = question.Replace("___", "<input type='text' class='form-control d-inline-block mx-1' style='width: 100px;'>");
            return new HtmlString(processedQuestion);
        }
        
        // Extension method để lấy tên đầy đủ của loại từ
        public static string GetPartOfSpeechName(this IHtmlHelper html, string? code)
        {
            if (string.IsNullOrEmpty(code))
                return "Không xác định";
                
            var types = new Dictionary<string, string>
            {
                { "n", "Danh từ (Noun)" },
                { "v", "Động từ (Verb)" },
                { "adj", "Tính từ (Adjective)" },
                { "adv", "Trạng từ (Adverb)" },
                { "prep", "Giới từ (Preposition)" },
                { "conj", "Liên từ (Conjunction)" },
                { "pron", "Đại từ (Pronoun)" },
                { "det", "Hạn định từ (Determiner)" },
                { "interj", "Thán từ (Interjection)" }
            };
            
            return types.ContainsKey(code) ? types[code] : code;
        }
        
        // Extension method để lấy nội dung tiếng Việt
        public static string GetVietnameseContent(this IHtmlHelper html, string? englishContent)
        {
            if (string.IsNullOrEmpty(englishContent))
                return "Không có nội dung";
                
            // Trong thực tế đây có thể là lời gọi đến API dịch hoặc database
            return englishContent;
        }
    }
}
