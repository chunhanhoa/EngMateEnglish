using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TiengAnh.Models;

namespace TiengAnh.Data;

public partial class HocTiengAnhContext : DbContext
{
    public HocTiengAnhContext()
    {
    }

    public HocTiengAnhContext(DbContextOptions<HocTiengAnhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiTap> BaiTaps { get; set; }

    public virtual DbSet<CauHoiKt> CauHoiKts { get; set; }

    public virtual DbSet<ChiTietKetQua> ChiTietKetQuas { get; set; }

    public virtual DbSet<ChuDe> ChuDes { get; set; }

    public virtual DbSet<KetQuaKiemTra> KetQuaKiemTras { get; set; }

    public virtual DbSet<KiemTra> KiemTras { get; set; }

    public virtual DbSet<LoaiTu> LoaiTus { get; set; }

    public virtual DbSet<NguPhap> NguPhaps { get; set; }

    public virtual DbSet<PhanQuyen> PhanQuyens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TienTrinhHoc> TienTrinhHocs { get; set; }

    public virtual DbSet<TuVung> TuVungs { get; set; }

    public virtual DbSet<YeuThich> YeuThiches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Chỉ sử dụng connection string nếu chưa có options được cấu hình
        if (!optionsBuilder.IsConfigured)
        {
            // Cảnh báo này có thể bỏ qua vì chúng ta đã cấu hình connection string trong Program.cs
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.HasKey(e => e.IdBt).HasName("PK__BaiTap__8B6207FCC79AE5AD");

            entity.ToTable("BaiTap");

            entity.Property(e => e.IdBt).HasColumnName("ID_BT");
            entity.Property(e => e.AnswerBt)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("answer_BT");
            entity.Property(e => e.ExplanationBt)
                .HasMaxLength(255)
                .HasColumnName("explanation_BT");
            entity.Property(e => e.IdCd).HasColumnName("ID_CD");
            entity.Property(e => e.OptionA)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_A");
            entity.Property(e => e.OptionB)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_B");
            entity.Property(e => e.OptionC)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_C");
            entity.Property(e => e.OptionD)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_D");
            entity.Property(e => e.QuestionBt)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("question_BT");

            entity.HasOne(d => d.IdCdNavigation).WithMany(p => p.BaiTaps)
                .HasForeignKey(d => d.IdCd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exercises_ChuDe");
        });

        modelBuilder.Entity<CauHoiKt>(entity =>
        {
            entity.HasKey(e => e.IdCh).HasName("PK__CauHoiKT__8B622F83FEFF83EE");

            entity.ToTable("CauHoiKT");

            entity.Property(e => e.IdCh).HasColumnName("ID_CH");
            entity.Property(e => e.AnswerCh)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("answer_CH");
            entity.Property(e => e.ExplanationCh)
                .HasMaxLength(255)
                .HasColumnName("explanation_BT");
            entity.Property(e => e.IdKt).HasColumnName("ID_KT");
            entity.Property(e => e.OptionA)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_A");
            entity.Property(e => e.OptionB)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_B");
            entity.Property(e => e.OptionC)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_C");
            entity.Property(e => e.OptionD)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("option_D");
            entity.Property(e => e.QuestionCh)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("question_CH");

            entity.HasOne(d => d.IdKtNavigation).WithMany(p => p.CauHoiKts)
                .HasForeignKey(d => d.IdKt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestQuestions_Test");
        });

        modelBuilder.Entity<ChiTietKetQua>(entity =>
        {
            entity.HasKey(e => e.IdCtkq).HasName("PK__ChiTietK__7FD0FD811FECDAC1");

            entity.ToTable("ChiTietKetQua");

            entity.Property(e => e.IdCtkq).HasColumnName("ID_CTKQ");
            entity.Property(e => e.IdCh).HasColumnName("ID_CH");
            entity.Property(e => e.IdKq).HasColumnName("ID_KQ");
            entity.Property(e => e.IsCorrectCtkq).HasColumnName("is_correct_CTKQ");
            entity.Property(e => e.UserAnswerCtkq)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("user_answer_CTKQ");

            entity.HasOne(d => d.IdChNavigation).WithMany(p => p.ChiTietKetQuas)
                .HasForeignKey(d => d.IdCh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietKetQua_CauHoi");

            entity.HasOne(d => d.IdKqNavigation).WithMany(p => p.ChiTietKetQuas)
                .HasForeignKey(d => d.IdKq)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietKetQua_KetQua");
        });

        modelBuilder.Entity<ChuDe>(entity =>
        {
            entity.HasKey(e => e.IdCd).HasName("PK__ChuDe__8B622F8FACA9E0F4");

            entity.ToTable("ChuDe");

            entity.Property(e => e.IdCd).HasColumnName("ID_CD");
            entity.Property(e => e.DiscriptionCd)
                .HasMaxLength(255)
                .HasColumnName("discription_CD");
            entity.Property(e => e.NameCd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name_CD");
        });

        modelBuilder.Entity<KetQuaKiemTra>(entity =>
        {
            entity.HasKey(e => e.IdKq).HasName("PK__KetQuaKi__8B62EC90AF9F117D");

            entity.ToTable("KetQuaKiemTra");

            entity.Property(e => e.IdKq).HasColumnName("ID_KQ");
            entity.Property(e => e.FinishTimeKq)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("finish_time_KQ");
            entity.Property(e => e.IdKt).HasColumnName("ID_KT");
            entity.Property(e => e.IdTk).HasColumnName("ID_TK");
            entity.Property(e => e.ScoreKq).HasColumnName("score_KQ");

            entity.HasOne(d => d.IdKtNavigation).WithMany(p => p.KetQuaKiemTras)
                .HasForeignKey(d => d.IdKt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaKiemTra_KiemTra");

            entity.HasOne(d => d.IdTkNavigation).WithMany(p => p.KetQuaKiemTras)
                .HasForeignKey(d => d.IdTk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaKiemTra_TaiKhoan");
        });

        modelBuilder.Entity<KiemTra>(entity =>
        {
            entity.HasKey(e => e.IdKt).HasName("PK__KiemTra__8B62EC95CE5A62C1");

            entity.ToTable("KiemTra");

            entity.Property(e => e.IdKt).HasColumnName("ID_KT");
            entity.Property(e => e.IdCd).HasColumnName("ID_CD");
            entity.Property(e => e.TitleKt)
                .HasMaxLength(100)
                .HasColumnName("title_KT");

            entity.HasOne(d => d.IdCdNavigation).WithMany(p => p.KiemTras)
                .HasForeignKey(d => d.IdCd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KiemTra_ChuDe");
        });

        modelBuilder.Entity<LoaiTu>(entity =>
        {
            entity.HasKey(e => e.IdLt).HasName("PK__LoaiTu__8B62F4B414E49045");

            entity.ToTable("LoaiTu");

            entity.Property(e => e.IdLt)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ID_LT");
            entity.Property(e => e.ExampleLt)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("example_LT");
            entity.Property(e => e.ExplainLt)
                .HasMaxLength(255)
                .HasColumnName("explain_LT");
            entity.Property(e => e.NameLt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("name_LT");
        });

        modelBuilder.Entity<NguPhap>(entity =>
        {
            entity.HasKey(e => e.IdNp).HasName("PK__NguPhap__8B63E06953A29167");

            entity.ToTable("NguPhap");

            entity.Property(e => e.IdNp).HasColumnName("ID_NP");
            entity.Property(e => e.DiscriptionNp)
                .HasMaxLength(255)
                .HasColumnName("discription_NP");
            entity.Property(e => e.IdCd).HasColumnName("ID_CD");
            entity.Property(e => e.ContentNp)
                .HasColumnName("content_NP");
            entity.Property(e => e.TimeuploadNp).HasColumnName("timeupload_NP");
            entity.Property(e => e.TitleNp)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("title_NP");

            entity.HasOne(d => d.IdCdNavigation).WithMany(p => p.NguPhaps)
                .HasForeignKey(d => d.IdCd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grammar_ChuDe");
        });

        modelBuilder.Entity<PhanQuyen>(entity =>
        {
            entity.HasKey(e => e.IdQ).HasName("PK__PhanQuye__B87EA51BFB6360B8");

            entity.ToTable("PhanQuyen");

            entity.Property(e => e.IdQ)
                .ValueGeneratedNever()
                .HasColumnName("ID_Q");
            entity.Property(e => e.NameQ)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("name_Q");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.IdTk).HasName("PK__TaiKhoan__8B63B1A9FB1D28E7");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.IdTk).HasColumnName("ID_TK");
            entity.Property(e => e.DisplaynameTk)
                .HasMaxLength(50)
                .HasColumnName("displayname_TK");
            entity.Property(e => e.EmailTk)
                .HasMaxLength(50)
                .HasColumnName("email_TK");
            entity.Property(e => e.IdQ).HasColumnName("ID_Q");
            entity.Property(e => e.NameTk)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name_TK");
            entity.Property(e => e.PasswordTk)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password_TK");
            entity.Property(e => e.PhoneTk)
                .HasMaxLength(12)
                .HasColumnName("phone_TK");
            entity.Property(e => e.AvatarTk)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar_TK");

            entity.HasOne(d => d.IdQNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.IdQ)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiKhoan_PhanQuyen");
        });

        modelBuilder.Entity<TienTrinhHoc>(entity =>
        {
            entity.HasKey(e => e.IdTth).HasName("PK__TienTrin__27BF88B2A1013737");

            entity.ToTable("TienTrinhHoc");

            entity.Property(e => e.IdTth).HasColumnName("ID_TTH");
            entity.Property(e => e.IdTk).HasColumnName("ID_TK");
            entity.Property(e => e.IdTypeTth).HasColumnName("ID_type_TTH");
            entity.Property(e => e.LastTimeStudyTth)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_time_study_TTH");
            entity.Property(e => e.StatusTth)
                .HasMaxLength(50)
                .HasColumnName("status_TTH");
            entity.Property(e => e.TypeTth)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("type_TTH");

            entity.HasOne(d => d.IdTkNavigation).WithMany(p => p.TienTrinhHocs)
                .HasForeignKey(d => d.IdTk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienTrinhHoc_TaiKhoan");
        });

        modelBuilder.Entity<TuVung>(entity =>
        {
            entity.HasKey(e => e.IdTv).HasName("PK__TuVung__8B63B1A4CFCF9C84");

            entity.ToTable("TuVung");

            entity.Property(e => e.IdTv).HasColumnName("ID_TV");
            entity.Property(e => e.AudioTv)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("audio_TV");
            entity.Property(e => e.ExampleTv)
                .HasMaxLength(255)
                .HasColumnName("example_TV");
            entity.Property(e => e.IdCd).HasColumnName("ID_CD");
            entity.Property(e => e.IdLt)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ID_LT");
            entity.Property(e => e.ImageTv)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image_TV");
            entity.Property(e => e.LevelTv)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("level_TV");
            entity.Property(e => e.MeaningTv)
                .HasMaxLength(100)
                .HasColumnName("meaning_TV");
            entity.Property(e => e.WordTv)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("word_TV");

            entity.HasOne(d => d.IdCdNavigation).WithMany(p => p.TuVungs)
                .HasForeignKey(d => d.IdCd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TuVung_ChuDe");

            entity.HasOne(d => d.IdLtNavigation).WithMany(p => p.TuVungs)
                .HasForeignKey(d => d.IdLt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TuVung_LoaiTu");
        });

        modelBuilder.Entity<YeuThich>(entity =>
        {
            entity.HasKey(e => e.IdYt).HasName("PK__YeuThich__8B627ECBAC60AF7D");

            entity.ToTable("YeuThich");

            entity.Property(e => e.IdYt).HasColumnName("ID_YT");
            entity.Property(e => e.DateCheckYt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date_check_YT");
            entity.Property(e => e.IdTk).HasColumnName("ID_TK");
            entity.Property(e => e.IdTypeYt).HasColumnName("ID_type_YT");
            entity.Property(e => e.TypeYt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("type_YT");

            entity.HasOne(d => d.IdTkNavigation).WithMany(p => p.YeuThiches)
                .HasForeignKey(d => d.IdTk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MucYeuThich_TaiKhoan");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
