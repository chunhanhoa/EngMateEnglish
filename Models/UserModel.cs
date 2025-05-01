using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TiengAnh.Models
{
    [BsonIgnoreExtraElements]
    public class UserModel : BaseModel
    {
        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;
        
        [BsonElement("Username")]
        public string Username { get; set; } = string.Empty;
        
        [BsonElement("UserName")]
        public string UserName 
        { 
            get { return Username; }
            set { Username = value; }
        }
        
        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;
        
        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;
        
        [BsonElement("FullName")]
        public string FullName { get; set; } = string.Empty;
        
        [BsonElement("Avatar")]
        public string? Avatar { get; set; }
        
        [BsonElement("Level")]
        public string Level { get; set; } = "A1";
        
        [BsonElement("Role")]
        public string Role { get; set; } = "User";
        
        [BsonElement("Roles")]
        public List<string> Roles { get; set; } = new List<string>();
        
        [BsonElement("RegisterDate")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        
        [BsonElement("Points")]
        public int Points { get; set; }
        
        [BsonElement("Phone")]
        public string Phone { get; set; } = string.Empty;
        
        [BsonElement("Address")]
        public string Address { get; set; } = string.Empty;
        
        [BsonElement("DateOfBirth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [BsonElement("Gender")]
        public string Gender { get; set; } = string.Empty;
        
        [BsonElement("Bio")]
        public string Bio { get; set; } = string.Empty;
        
        [BsonElement("Specialization")]
        public string Specialization { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("LastLogin")]
        public DateTime? LastLogin { get; set; }

        [BsonElement("IsVerified")]
        public bool IsVerified { get; set; } = false;
    }
    
    // Xóa các lớp model trùng lặp ở đây để tránh xung đột
    // LoginViewModel và RegisterViewModel đã được chuyển vào namespace Models.ViewModels
}
