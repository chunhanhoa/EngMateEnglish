using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class NguPhap
{
    public int IdNp { get; set; }
    public string? TitleNp { get; set; }
    public string? DiscriptionNp { get; set; }
    // Cột ContentNp - cần đảm bảo có trong DB
    public string? ContentNp { get; set; }
    public int IdCd { get; set; }
    public DateTime? TimeuploadNp { get; set; }
    public virtual ChuDe IdCdNavigation { get; set; } = null!;
}
