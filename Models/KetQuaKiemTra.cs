using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class KetQuaKiemTra
{
    public int IdKq { get; set; }

    public int ScoreKq { get; set; }

    public DateTime? FinishTimeKq { get; set; }

    public int IdTk { get; set; }

    public int IdKt { get; set; }

    public virtual ICollection<ChiTietKetQua> ChiTietKetQuas { get; set; } = new List<ChiTietKetQua>();

    public virtual KiemTra IdKtNavigation { get; set; } = null!;

    public virtual TaiKhoan IdTkNavigation { get; set; } = null!;
}
