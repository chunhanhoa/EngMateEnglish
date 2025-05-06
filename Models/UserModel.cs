using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; }
        
        // Add a dedicated field for Google IDs that is not marked as the document ID
        [BsonElement("GoogleId")]
        public string GoogleId { get; set; }
        
        // For compatibility with existing code
        [BsonElement("UserId")]
        public string UserId { get; set; }
        
        [BsonElement("Email")]
        public string Email { get; set; }
        
        [BsonElement("Username")]
        public string Username { get; set; }
        
        [BsonElement("UserName")]
        public string UserName { get; set; }
        
        [BsonElement("FullName")]
        public string FullName { get; set; }
        
        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }
        
        [BsonElement("Avatar")]
        public string Avatar { get; set; }
        
        [BsonElement("Role")]
        public string Role { get; set; }
        
        [BsonElement("Roles")]
        public List<string> Roles { get; set; }
        
        [BsonElement("Level")]
        public string Level { get; set; }
        
        [BsonElement("Points")]
        public int Points { get; set; }
        
        [BsonElement("Phone")]
        public string Phone { get; set; }
        
        [BsonElement("Gender")]
        public string Gender { get; set; }
        
        [BsonElement("Address")]
        public string Address { get; set; }
        
        [BsonElement("Bio")]
        public string Bio { get; set; }
        
        [BsonElement("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }
        
        [BsonElement("CreatedAt")]
        public DateTime? CreatedAt { get; set; }
        
        [BsonElement("RegisterDate")]
        public DateTime RegisterDate { get; set; }
        
        [BsonElement("LastLogin")]
        public DateTime? LastLogin { get; set; }
        
        [BsonElement("IsVerified")]
        public bool IsVerified { get; set; }
    }
}
