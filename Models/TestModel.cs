namespace TiengAnh.Models
{
    public class TestModel
    {
        public int TestID { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int TopicID { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public string Level { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class QuestionModel
    {
        public int QuestionID { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
