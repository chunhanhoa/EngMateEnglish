using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TiengAnh.Models
{
    public abstract class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; } = string.Empty;

        [BsonIgnore]
        public string StringId => Id;
    }
}
