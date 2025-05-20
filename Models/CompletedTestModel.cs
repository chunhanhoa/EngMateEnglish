using System;

namespace TiengAnh.Models
{
    public class CompletedTestModel
    {
        public string TestId { get; set; }
        public string TestTitle { get; set; }
        public string TestCategory { get; set; }
        public string TestLevel { get; set; }
        public string ImageUrl { get; set; }
        public int Score { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public string TimeTaken { get; set; }
        public DateTime CompletedAt { get; set; }
        
        // Add this property for the Detail URL
        public string DetailUrl { get; set; }
    }
}
