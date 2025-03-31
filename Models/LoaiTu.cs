using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class LoaiTu
{
    public string IdLt { get; set; } = null!;

    public string NameLt { get; set; } = null!;

    public string? ExplainLt { get; set; }

    public string? ExampleLt { get; set; }

    public virtual ICollection<TuVung> TuVungs { get; set; } = new List<TuVung>();
}
