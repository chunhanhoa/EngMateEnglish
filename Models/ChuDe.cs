using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class ChuDe
{
    public int IdCd { get; set; }

    public string NameCd { get; set; } = null!;

    public string? DiscriptionCd { get; set; }

    public virtual ICollection<BaiTap> BaiTaps { get; set; } = new List<BaiTap>();

    public virtual ICollection<KiemTra> KiemTras { get; set; } = new List<KiemTra>();

    public virtual ICollection<NguPhap> NguPhaps { get; set; } = new List<NguPhap>();

    public virtual ICollection<TuVung> TuVungs { get; set; } = new List<TuVung>();
}
