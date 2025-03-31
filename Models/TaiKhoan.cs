using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class TaiKhoan
{
    public int IdTk { get; set; }

    public string NameTk { get; set; } = null!;

    public string PasswordTk { get; set; } = null!;

    public string? DisplaynameTk { get; set; }

    public string EmailTk { get; set; } = null!;

    public string? PhoneTk { get; set; }

    // Trường mới để lưu đường dẫn ảnh đại diện
    public string? AvatarTk { get; set; }

    public int IdQ { get; set; }

    public virtual PhanQuyen IdQNavigation { get; set; } = null!;

    public virtual ICollection<KetQuaKiemTra> KetQuaKiemTras { get; set; } = new List<KetQuaKiemTra>();

    public virtual ICollection<TienTrinhHoc> TienTrinhHocs { get; set; } = new List<TienTrinhHoc>();

    public virtual ICollection<YeuThich> YeuThiches { get; set; } = new List<YeuThich>();
}
