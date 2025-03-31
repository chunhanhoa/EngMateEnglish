namespace TiengAnh.Models
{
    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public int Points { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        
        // Thêm thông tin học tập thực tế
        public int VocabularyProgress { get; set; }
        public int GrammarProgress { get; set; }
        public int ExerciseProgress { get; set; }
        public List<UserActivity> RecentActivities { get; set; } = new List<UserActivity>();
    }

    public class UserActivity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool AcceptTerms { get; set; }
    }
}
