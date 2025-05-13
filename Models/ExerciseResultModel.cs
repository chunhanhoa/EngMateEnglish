using System;

namespace TiengAnh.Models
{
    public class ExerciseResultModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletionDate { get; set; }
        public string ExerciseType { get; set; }
    }
}
