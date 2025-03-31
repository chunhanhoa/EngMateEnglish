using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class TuVung
{
    public int IdTv { get; set; }

    public string WordTv { get; set; } = null!;

    public string MeaningTv { get; set; } = null!;

    public string ExampleTv { get; set; } = null!;

    public string? AudioTv { get; set; }

    public string? ImageTv { get; set; }

    public string? LevelTv { get; set; }

    public int IdCd { get; set; }

    public string IdLt { get; set; } = null!;

    public virtual ChuDe IdCdNavigation { get; set; } = null!;

    public virtual LoaiTu IdLtNavigation { get; set; } = null!;
}
