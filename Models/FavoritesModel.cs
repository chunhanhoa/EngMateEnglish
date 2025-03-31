using System;

namespace TiengAnh.Models
{
    public class YeuThichViewModel
    {
        public int ID { get; set; }
        public string Type { get; set; } = string.Empty;
        public int ItemID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
    }
    
    // Request/Response models cho AJAX
    public class ToggleFavoriteRequest
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; } = string.Empty;
    }
    
    public class FavoriteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}
