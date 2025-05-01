using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class VocabularyModel : BaseModel
    {
        [BsonElement("ID_TV")]
        public int ID_TV { get; set; }
        
        [BsonElement("Word_TV")]
        public string Word_TV { get; set; } = null!;
        
        [BsonElement("Meaning_TV")]
        public string Meaning_TV { get; set; } = null!;
        
        [BsonElement("Example_TV")]
        public string? Example_TV { get; set; }
        
        [BsonElement("Audio_TV")]
        public string? Audio_TV { get; set; }
        
        [BsonElement("Image_TV")]
        public string? Image_TV { get; set; }
        
        [BsonElement("Level_TV")]
        public string Level_TV { get; set; } = null!;
        
        [BsonElement("ID_CD")]
        public int ID_CD { get; set; }
        
        [BsonElement("ID_LT")]
        public string? ID_LT { get; set; }
        
        [BsonElement("PartOfSpeech")]
        public string? PartOfSpeech { get; set; }
        
        [BsonElement("TopicName")]
        public string TopicName { get; set; } = null!;
        
        [BsonElement("IsFavorite")]
        public bool IsFavorite { get; set; }
        
        [BsonElement("FavoriteByUsers")]
        public List<string> FavoriteByUsers { get; set; } = new List<string>();
        
        public bool IsFavoriteByUser(string userId)
        {
            return FavoriteByUsers != null && FavoriteByUsers.Contains(userId);
        }
    }
}
