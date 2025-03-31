using System;
using System.Collections.Generic;

namespace TiengAnh.Models;

public partial class BaiTap
{
    public int IdBt { get; set; }

    public string QuestionBt { get; set; } = null!;

    public string? OptionA { get; set; }

    public string? OptionB { get; set; }

    public string? OptionC { get; set; }

    public string? OptionD { get; set; }

    public string AnswerBt { get; set; } = null!;

    public string? ExplanationBt { get; set; }

    public int IdCd { get; set; }

    public virtual ChuDe IdCdNavigation { get; set; } = null!;
}
