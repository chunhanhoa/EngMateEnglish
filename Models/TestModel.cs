using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TiengAnh.Models
{
    public class TestModel : BaseModel
    {
        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("duration")]
        public int Duration { get; set; }

        [BsonElement("level")]
        public string Level { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        // Additional property for compatibility with DataSeeder and TestModelConverter
        [BsonIgnore]
        public DateTime CreatedDate 
        { 
            get { return CreatedAt; }
            set { CreatedAt = value; }
        }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Additional property for compatibility with DataSeeder and TestModelConverter
        [BsonIgnore]
        public DateTime UpdatedDate 
        { 
            get { return UpdatedAt; }
            set { UpdatedAt = value; }
        }

        [BsonElement("category")]
        public string Category { get; set; } = null!;

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = null!;

        [BsonElement("questions")]
        public List<TestQuestionModel> Questions { get; set; } = new List<TestQuestionModel>();
        
        [BsonElement("testIdentifier")]
        public string TestIdentifier { get; set; } = null!;
        
        [BsonElement("jsonId")]
        public string JsonId { get; set; } = null!;
        
        // Additional properties for compatibility with Views
        [BsonIgnore]
        public string TestID 
        {
            get { return TestIdentifier; }
            set { TestIdentifier = value; }
        }
        
        [BsonIgnore]
        public string TestName
        {
            get { return Title; }
            set { Title = value; }
        }
        
        [BsonIgnore]
        public CompletedTestModel CompletionInfo { get; set; }
        
        // Add OnDeserialized method for compatibility with DataSeeder
        public void OnDeserialized()
        {
            // Ensure TestIdentifier is set
            if (string.IsNullOrEmpty(TestIdentifier) && !string.IsNullOrEmpty(JsonId))
            {
                TestIdentifier = JsonId;
            }
            else if (string.IsNullOrEmpty(TestIdentifier) && !string.IsNullOrEmpty(Id))
            {
                TestIdentifier = $"test_{Id.Substring(Math.Max(0, Id.Length - 6))}";
            }
        }
    }

    public class TestQuestionModel
    {
        [BsonElement("questionId")]
        [JsonPropertyName("questionId")] // Explicit JSON property name
        public int QuestionId { get; set; }
        
        // Remove the property causing collisions and use methods instead
        // Methods don't need serialization attributes since they're not serialized
        public int GetQuestionID() 
        {
            return QuestionId;
        }
        
        public void SetQuestionID(int id)
        {
            QuestionId = id;
        }

        [BsonElement("questionText")]
        public string QuestionText { get; set; } = null!;

        [BsonElement("options")]
        public List<string> Options { get; set; } = new List<string>();

        [BsonElement("correctAnswer")]
        public int CorrectAnswer { get; set; }
    }
}