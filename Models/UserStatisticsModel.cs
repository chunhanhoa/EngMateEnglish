using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TiengAnh.Models
{
    public class UserStatisticsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("totalUsers")]
        public int TotalUsers { get; set; }

        [BsonElement("activeUsers")]
        public int ActiveUsers { get; set; }

        [BsonElement("newUsersToday")]
        public int NewUsersToday { get; set; }

        [BsonElement("newUsersThisWeek")]
        public int NewUsersThisWeek { get; set; }

        [BsonElement("newUsersThisMonth")]
        public int NewUsersThisMonth { get; set; }

        [BsonElement("usersByRole")]
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();

        [BsonElement("usersByLevel")]
        public Dictionary<string, int> UsersByLevel { get; set; } = new Dictionary<string, int>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class UserActivityModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("lastLogin")]
        public DateTime LastLogin { get; set; }

        [BsonElement("lastActivity")]
        public DateTime LastActivity { get; set; }

        [BsonElement("isOnline")]
        public bool IsOnline { get; set; }

        [BsonElement("totalSessions")]
        public int TotalSessions { get; set; }

        [BsonElement("totalTimeSpent")]
        public TimeSpan TotalTimeSpent { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class DashboardStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int ActiveUsersThisWeek { get; set; }
        public int ActiveUsersThisMonth { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UsersByLevel { get; set; } = new Dictionary<string, int>();
        public List<UserActivitySummary> RecentUsers { get; set; } = new List<UserActivitySummary>();
        public List<ChartDataPoint> UserGrowthChart { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> ActivityChart { get; set; } = new List<ChartDataPoint>();
    }

    public class UserActivitySummary
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; } // Online, Offline, New
        public int DaysActive { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }
}
