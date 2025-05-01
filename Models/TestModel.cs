using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class TestModel : BaseModel
    {
        [BsonElement("TestID")]
        public int TestID { get; set; }
        
        [BsonElement("Title")]
        public string Title { get; set; } = null!;
        
        [BsonElement("TestName")]
        public string TestName
        {
            get { return Title; }
            set { Title = value; }
        }
        
        [BsonElement("Description")]
        public string Description { get; set; } = null!;
        
        [BsonElement("Duration")]
        public int Duration { get; set; }
        
        [BsonElement("TotalQuestions")]
        public int TotalQuestions { get; set; }
        
        [BsonElement("Level")]
        public string Level { get; set; } = null!;
        
        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; }
        
        [BsonElement("Questions")]
        public List<TestQuestionModel> Questions { get; set; } = new List<TestQuestionModel>();
        
        [BsonElement("ImageUrl")]
        public string? ImageUrl { get; set; }
        
        [BsonElement("Category")]
        public string? Category { get; set; }
    }
    
    public class TestQuestionModel
    {
        [BsonElement("QuestionID")]
        public int QuestionID { get; set; }
        
        [BsonElement("QuestionText")]
        public string QuestionText { get; set; } = null!;
        
        [BsonElement("Options")]
        public List<string> Options { get; set; } = new List<string>();
        
        [BsonElement("CorrectAnswer")]
        public int CorrectAnswer { get; set; }
    }
}
