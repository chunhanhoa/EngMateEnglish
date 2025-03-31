using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiengAnh.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarColumnToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // Xóa tất cả các lệnh tạo bảng, chỉ giữ lại lệnh thêm cột AvatarTk
            migrationBuilder.AddColumn<string>(
                name: "avatar_TK",
                table: "TaiKhoan",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiTap");

            migrationBuilder.DropTable(
                name: "ChiTietKetQua");

            migrationBuilder.DropTable(
                name: "NguPhap");

            migrationBuilder.DropTable(
                name: "TienTrinhHoc");

            migrationBuilder.DropTable(
                name: "TuVung");

            migrationBuilder.DropTable(
                name: "YeuThich");

            migrationBuilder.DropTable(
                name: "CauHoiKT");

            migrationBuilder.DropTable(
                name: "KetQuaKiemTra");

            migrationBuilder.DropTable(
                name: "LoaiTu");

            migrationBuilder.DropTable(
                name: "KiemTra");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "ChuDe");

            migrationBuilder.DropTable(
                name: "PhanQuyen");
        }
    }
}
