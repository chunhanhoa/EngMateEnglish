using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiengAnh.Models;

public partial class YeuThich
{
    public int IdYt { get; set; }

    public string TypeYt { get; set; } = null!;

    public DateTime? DateCheckYt { get; set; }

    public int IdTk { get; set; }

    public int IdTypeYt { get; set; }
    
    // Thêm thuộc tính này như một alias, ánh xạ đến cùng một cột trong cơ sở dữ liệu
    [NotMapped]
    public int IdYtType 
    { 
        get => IdTypeYt; 
        set => IdTypeYt = value; 
    }

    public virtual TaiKhoan IdTkNavigation { get; set; } = null!;
}
