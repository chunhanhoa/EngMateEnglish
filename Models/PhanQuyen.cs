using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class PhanQuyen
{
    public int IdQ { get; set; }

    public string NameQ { get; set; } = null!;

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
