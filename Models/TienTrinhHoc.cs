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

    public virtual TaiKhoan IdTkNavigation { get; set; } = null!;
}
