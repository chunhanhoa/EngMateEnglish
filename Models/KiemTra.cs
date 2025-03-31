using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class KiemTra
{
    public int IdKt { get; set; }

    public string TitleKt { get; set; } = null!;

    public int IdCd { get; set; }

    public virtual ICollection<CauHoiKt> CauHoiKts { get; set; } = new List<CauHoiKt>();

    public virtual ChuDe IdCdNavigation { get; set; } = null!;

    public virtual ICollection<KetQuaKiemTra> KetQuaKiemTras { get; set; } = new List<KetQuaKiemTra>();
}
