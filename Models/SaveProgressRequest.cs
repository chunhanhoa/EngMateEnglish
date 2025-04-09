namespace TiengAnh.Models
{
    public class SaveProgressRequest
    {
        public int ItemId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Score { get; set; } = 0;
        public int TopicId { get; set; }
    }
}
