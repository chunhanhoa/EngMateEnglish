using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo.Models
{
    // Test model
    public class KiemTra
    {
        [Key]
        public int ID_KT { get; set; }
        
        [Required]
        [StringLength(100)]
        public string title_KT { get; set; }
        
        public int ID_CD { get; set; }
        
        [ForeignKey("ID_CD")]
        public virtual ChuDe ChuDe { get; set; }
        
        public virtual ICollection<CauHoiKT> CauHoiKT { get; set; }
        public virtual ICollection<KetQuaKiemTra> KetQuaKiemTra { get; set; }
    }
    
    // Topic model
    public class ChuDe
    {
        [Key]
        public int ID_CD { get; set; }
        
        [Required]
        [StringLength(50)]
        public string name_CD { get; set; }
        
        [StringLength(1000)]
        public string discription_CD { get; set; }
        
        public virtual ICollection<KiemTra> KiemTra { get; set; }
    }
    
    // Test question model
    public class CauHoiKT
    {
        [Key]
        public int ID_CH { get; set; }
        
        [Required]
        [StringLength(255)]
        public string question_CH { get; set; }
        
        [StringLength(255)]
        public string option_A { get; set; }
        
        [StringLength(255)]
        public string option_B { get; set; }
        
        [StringLength(255)]
        public string option_C { get; set; }
        
        [StringLength(255)]
        public string option_D { get; set; }
        
        [Required]
        [StringLength(1)]
        public string answer_CH { get; set; }
        
        [StringLength(255)]
        public string explanation_CH { get; set; }
        
        public int ID_KT { get; set; }
        
        [ForeignKey("ID_KT")]
        public virtual KiemTra KiemTra { get; set; }
        
        public virtual ICollection<ChiTietKetQua> ChiTietKetQua { get; set; }
    }
    
    // Test result model
    public class KetQuaKiemTra
    {
        [Key]
        public int ID_KQ { get; set; }
        
        [Required]
        public int score_KQ { get; set; }
        
        public DateTime finish_time_KQ { get; set; }
        
        public int ID_TK { get; set; }
        
        public int ID_KT { get; set; }
        
        [ForeignKey("ID_TK")]
        public virtual TaiKhoan TaiKhoan { get; set; }
        
        [ForeignKey("ID_KT")]
        public virtual KiemTra KiemTra { get; set; }
        
        public virtual ICollection<ChiTietKetQua> ChiTietKetQua { get; set; }
    }
    
    // Test answer detail model
    public class ChiTietKetQua
    {
        [Key]
        public int ID_CTKQ { get; set; }
        
        [StringLength(1)]
        public string user_answer_CTKQ { get; set; }
        
        public bool is_correct_CTKQ { get; set; }
        
        public int ID_KQ { get; set; }
        
        public int ID_CH { get; set; }
        
        [ForeignKey("ID_KQ")]
        public virtual KetQuaKiemTra KetQuaKiemTra { get; set; }
        
        [ForeignKey("ID_CH")]
        public virtual CauHoiKT CauHoiKT { get; set; }
    }
    
    // User model (simplified)
    public class TaiKhoan
    {
        [Key]
        public int ID_TK { get; set; }
        
        [Required]
        [StringLength(50)]
        public string name_TK { get; set; }
        
        [Required]
        [StringLength(50)]
        public string password_TK { get; set; }
        
        [StringLength(50)]
        public string displayname_TK { get; set; }
        
        [Required]
        [StringLength(50)]
        public string email_TK { get; set; }
        
        [StringLength(12)]
        public string phone_TK { get; set; }
        
        public int ID_Q { get; set; }
        
        public virtual ICollection<KetQuaKiemTra> KetQuaKiemTra { get; set; }
    }
}
