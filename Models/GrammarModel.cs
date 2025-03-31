using System;

namespace TiengAnh.Models
{
    public class GrammarModel
    {
        public int ID_NP { get; set; }
        public string Title_NP { get; set; } = string.Empty;
        public string Description_NP { get; set; } = string.Empty;
        public DateTime? TimeUpload_NP { get; set; }
        public int ID_CD { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string Level { get; set; } = "A1";
        public string Content { get; set; } = string.Empty;
        public string Content_NP { get; set; } = string.Empty;
        public string Examples { get; set; } = string.Empty;
        public string ExerciseUrl { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; } = 0;
    }
}
