namespace TiengAnh.Models
{
    public class VocabularyModel
    {
        public int ID_TV { get; set; }
        public string Word_TV { get; set; } = string.Empty;
        public string Meaning_TV { get; set; } = string.Empty;
        public string ID_LT { get; set; } = string.Empty;
        public string WordType { get; set; } = string.Empty;
        public string PartOfSpeech { get; set; } = string.Empty; // Added this property
        public string Example_TV { get; set; } = string.Empty;
        public string Image_TV { get; set; } = string.Empty;
        public string Audio_TV { get; set; } = string.Empty;
        public string Level_TV { get; set; } = string.Empty;
        public int ID_CD { get; set; }
        public string TopicName { get; set; } = string.Empty;
    }
}
