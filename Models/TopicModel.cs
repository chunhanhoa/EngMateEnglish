using MongoDB.Bson.Serialization.Attributes;

namespace TiengAnh.Models
{
    public class TopicModel : BaseModel
    {
        [BsonElement("ID_CD")]
        public int ID_CD { get; set; }
        
        [BsonElement("Name_CD")]
        public string Name_CD { get; set; } = string.Empty;
        
        [BsonElement("Description_CD")]
        public string? Description_CD { get; set; }
        
        [BsonElement("IconPath")]
        public string? IconPath { get; set; }
        
        [BsonElement("Image_CD")]
        public string? Image_CD { get; set; }
        
        [BsonElement("Level")]
        public string? Level { get; set; }
        
        [BsonElement("TotalItems")]
        public int TotalItems { get; set; }
        
        [BsonElement("TotalWords")]
        public int TotalWords { get; set; }
        
        [BsonElement("WordCount")]
        public int WordCount { get; set; }
        
        [BsonElement("BackgroundColor")]
        public string? BackgroundColor { get; set; }
        
        [BsonElement("Type_CD")]
        public string Type_CD { get; set; } = "Default";
        
        // Tính toán đường dẫn hình ảnh hoặc trả về hình ảnh mặc định
        public string GetIconPath() 
        {
            return string.IsNullOrEmpty(IconPath) ? "/images/topics/default.png" : IconPath;
        }
    }
}
