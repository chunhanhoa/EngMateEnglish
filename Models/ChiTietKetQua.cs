using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class ChiTietKetQua
{
    public int IdCtkq { get; set; }

    public string? UserAnswerCtkq { get; set; }

    public bool? IsCorrectCtkq { get; set; }

    public int IdKq { get; set; }

    public int IdCh { get; set; }

    public virtual CauHoiKt IdChNavigation { get; set; } = null!;

    public virtual KetQuaKiemTra IdKqNavigation { get; set; } = null!;
}
