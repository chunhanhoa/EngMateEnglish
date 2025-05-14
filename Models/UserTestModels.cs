using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TiengAnh.Models
{
    public class UserTestModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("userId")]
        public string UserId { get; set; }
        
        [BsonElement("testId")]
        public string TestId { get; set; }
        
        [BsonElement("testTitle")]
        public string TestTitle { get; set; }
        
        [BsonElement("testCategory")]
        public string TestCategory { get; set; }
        
        [BsonElement("testLevel")]
        public string TestLevel { get; set; }
        
        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; }
        
        [BsonElement("score")]
        public int Score { get; set; }
        
        [BsonElement("correctCount")]
        public int CorrectCount { get; set; }
        
        [BsonElement("totalQuestions")]
        public int TotalQuestions { get; set; }
        
        [BsonElement("timeTaken")]
        public string TimeTaken { get; set; }
        
        [BsonElement("completedAt")]
        public DateTime CompletedAt { get; set; }
    }
}
