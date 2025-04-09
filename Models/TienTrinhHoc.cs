using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class TienTrinhHoc
{
    public int IdTth { get; set; }

    public string TypeTth { get; set; } = null!;

    public string? StatusTth { get; set; }

    public DateTime? LastTimeStudyTth { get; set; }

    public int IdTk { get; set; }

    public int IdTypeTth { get; set; }
    
    // Thêm thuộc tính điểm số
    public int? ScoreTth { get; set; }
    
    // Thêm thuộc tính Id chủ đề
    public int? IdCd { get; set; }

    public virtual TaiKhoan IdTkNavigation { get; set; } = null!;
}
