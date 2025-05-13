using System.Text.Json.Serialization;

namespace TiengAnh.Models
{
    public class TestSubmissionModel
    {
        [JsonPropertyName("testId")]
        public string TestId { get; set; }
        
        [JsonPropertyName("userAnswers")]
        public int[] UserAnswers { get; set; }
        
        [JsonPropertyName("score")]
        public int Score { get; set; }
        
        [JsonPropertyName("correctCount")]
        public int CorrectCount { get; set; }
        
        [JsonPropertyName("timeUsed")]
        public int TimeUsed { get; set; }
        
        [JsonPropertyName("timeTaken")]
        public string TimeTaken { get; set; }
    }
}
