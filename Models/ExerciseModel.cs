namespace TiengAnh.Models
{
    public class ExerciseModel
    {
        public int ID_BT { get; set; }
        public string Question_BT { get; set; } = string.Empty;
        public string Option_A { get; set; } = string.Empty;
        public string Option_B { get; set; } = string.Empty;
        public string Option_C { get; set; } = string.Empty;
        public string Option_D { get; set; } = string.Empty;
        public string Answer_BT { get; set; } = string.Empty;
        public string Explanation_BT { get; set; } = string.Empty;
        public int ID_CD { get; set; }
    }
}
