namespace TiengAnh.Models
{
    public class VocabularyTopicModel
    {
        public int ID_CD { get; set; }
        public string Name_CD { get; set; } = string.Empty;
        public string Description_CD { get; set; } = string.Empty;
        public string IconPath { get; set; } = "/images/icons/topic-default.png";
        public string BackgroundColor { get; set; } = "#e6f6ff";
        public int WordCount { get; set; }
    }
}
