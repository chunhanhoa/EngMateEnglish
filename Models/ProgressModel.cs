using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class ProgressModel : BaseModel
    {
        [BsonElement("UserId")]
        public string UserId { get; set; } = null!;
        
        [BsonElement("VocabularyProgress")]
        public int VocabularyProgress { get; set; }
        
        [BsonElement("GrammarProgress")]
        public int GrammarProgress { get; set; }
        
        [BsonElement("ExerciseProgress")]
        public int ExerciseProgress { get; set; }
        
        [BsonElement("TotalPoints")]
        public int TotalPoints { get; set; }
        
        [BsonElement("Level")]
        public string Level { get; set; } = null!;
        
        [BsonElement("LastCompletedItems")]
        public List<LastCompletedItemModel> LastCompletedItems { get; set; } = new List<LastCompletedItemModel>();
        
        [BsonElement("CompletedTopics")]
        public List<CompletedTopicModel> CompletedTopics { get; set; } = new List<CompletedTopicModel>();
    }

    public class LastCompletedItemModel
    {
        [BsonElement("Id")]
        public int Id { get; set; }
        
        [BsonElement("Type")]
        public string Type { get; set; } = null!;
        
        [BsonElement("Title")]
        public string Title { get; set; } = null!;
        
        [BsonElement("CompletedDate")]
        public DateTime CompletedDate { get; set; }
        
        [BsonElement("Score")]
        public int Score { get; set; }
    }

    public class CompletedTopicModel
    {
        [BsonElement("TopicId")]
        public int TopicId { get; set; }
        
        [BsonElement("TopicName")]
        public string TopicName { get; set; } = null!;
        
        [BsonElement("CompletionPercentage")]
        public int CompletionPercentage { get; set; }
    }
}
