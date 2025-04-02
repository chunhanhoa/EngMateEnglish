# DoAnCoSo (EngMate)

## Giới thiệu

**DoAnCoSo (EngMate)** là một dự án web được phát triển theo kiến trúc MVC, cung cấp các tính năng liên quan đến học tiếng Anh, quản lý thông tin người dùng và các chức năng hỗ trợ quá trình học tập.  
Dự án được xây dựng sử dụng **MVC ASP.NET Core** với ngôn ngữ **C#**, cùng giao diện người dùng được thiết kế bằng **HTML**, **CSS** và **JavaScript**.

## Công nghệ sử dụng

- **Kiến trúc MVC:** Tổ chức mã nguồn theo mô hình Controllers, Models và Views.
- **ASP.NET Core:** Framework phát triển ứng dụng web.
- **C#:** Ngôn ngữ lập trình chính.
- **HTML/CSS/JavaScript:** Công nghệ xây dựng giao diện người dùng.
- **Entity Framework:** Quản lý truy cập và thao tác dữ liệu.
- **SQL Server:** Quản lý dữ liệu backend.

## Cài đặt

### Yêu cầu

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Một công cụ phát triển như [Visual Studio](https://visualstudio.microsoft.com/) hoặc [Visual Studio Code](https://code.visualstudio.com/)
- SQL Server hoặc cơ sở dữ liệu tương thích (nếu dự án sử dụng)

### Hướng dẫn cài đặt

1. **Clone repository:**

   ```bash
   git clone https://github.com/chunhanhoa/DoAnCoSo.git

2. **Mở dự án:**

    Mở file solution TiengAnh.sln bằng Visual Studio hoặc mở thư mục dự án bằng Visual Studio Code.

3. **Cấu hình dự án:**

    Kiểm tra và cập nhật file appsettings.json (hoặc appsettings.Development.json) để cấu hình kết nối đến cơ sở dữ liệu nếu cần.

4. **Build và chạy dự án:**

    Từ dòng lệnh, di chuyển vào thư mục dự án và chạy:
   ```bash
   dotnet build
   dotnet run

6. **Truy cập ứng dụng:**

    Mở trình duyệt và truy cập địa chỉ mặc định (ví dụ: http://localhost:5000) để sử dụng ứng dụng.
