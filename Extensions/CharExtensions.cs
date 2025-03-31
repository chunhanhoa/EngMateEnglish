namespace TiengAnh.Extensions
{
    public static class CharExtensions
    {
        public static string ToSafeString(this char? value)
        {
            return value?.ToString() ?? string.Empty;
        }
        
        public static string ToSafeString(this char value)
        {
            return value.ToString();
        }
    }
}
