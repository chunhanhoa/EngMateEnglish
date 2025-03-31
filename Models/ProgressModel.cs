namespace TiengAnh.Models
{
    public class ProgressModel
    {
        public string UserId { get; set; } = string.Empty;
        public int VocabularyProgress { get; set; } // Phần trăm từ vựng đã học
        public int GrammarProgress { get; set; } // Phần trăm ngữ pháp đã học
        public int ExerciseProgress { get; set; } // Phần trăm bài tập đã hoàn thành
        public int TotalPoints { get; set; }
        public string Level { get; set; } = string.Empty;
        public List<LastCompletedItemModel> LastCompletedItems { get; set; } = new List<LastCompletedItemModel>();
        public List<CompletedTopicModel> CompletedTopics { get; set; } = new List<CompletedTopicModel>();
    }

    public class LastCompletedItemModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public int Score { get; set; }
    }

    public class CompletedTopicModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public int CompletionPercentage { get; set; }
    }
}
