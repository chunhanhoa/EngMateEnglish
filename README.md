# DoAnCoSo (EngMate)

## Giới thiệu

**EngMate** là một ứng dụng web học tiếng Anh toàn diện, được phát triển theo kiến trúc MVC. Ứng dụng cung cấp nhiều tính năng giúp người học tiếng Anh cải thiện kỹ năng ngôn ngữ một cách hiệu quả thông qua các bài học từ vựng, ngữ pháp, bài tập thực hành và kiểm tra.

Dự án được xây dựng sử dụng **ASP.NET Core MVC** với ngôn ngữ **C#**, tích hợp cơ sở dữ liệu **MongoDB**, cùng giao diện người dùng được thiết kế bằng **HTML**, **CSS**, **Bootstrap** và **JavaScript**.

## Tính năng chính

- **Học từ vựng**: Danh sách từ vựng phong phú theo chủ đề và trình độ
- **Học ngữ pháp**: Các bài học ngữ pháp từ cơ bản đến nâng cao (A1-C1)
- **Bài tập thực hành**: Đa dạng loại bài tập (trắc nghiệm, điền khuyết, sắp xếp từ,...)
- **Kiểm tra đánh giá**: Các bài kiểm tra để đánh giá năng lực
- **Theo dõi tiến độ**: Hệ thống theo dõi tiến độ học tập của người dùng
- **Yêu thích**: Lưu từ vựng và ngữ pháp yêu thích để học lại

## Công nghệ sử dụng

- **ASP.NET Core MVC**: Framework phát triển web
- **C#**: Ngôn ngữ lập trình backend
- **MongoDB**: Cơ sở dữ liệu NoSQL
- **Bootstrap 5**: Framework CSS cho giao diện đáp ứng
- **Font Awesome**: Thư viện biểu tượng
- **JavaScript/jQuery**: Tương tác phía client
- **AJAX**: Giao tiếp bất đồng bộ với server

## Kiến trúc dự án

- **Models**: Định nghĩa cấu trúc dữ liệu (GrammarModel, VocabularyModel, TestModel,...)
- **Views**: Giao diện người dùng
- **Controllers**: Xử lý logic ứng dụng
- **Repositories**: Truy cập và thao tác dữ liệu
- **Services**: Xử lý các dịch vụ như kết nối MongoDB, seeding dữ liệu

## Cài đặt

### Yêu cầu

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [MongoDB](https://www.mongodb.com/try/download/community)
- Một công cụ phát triển như [Visual Studio](https://visualstudio.microsoft.com/) hoặc [Visual Studio Code](https://code.visualstudio.com/)

### Hướng dẫn cài đặt

1. **Clone repository:**

   ```bash
   git clone https://github.com/your-username/DoAnCoSo.git
   ```

2. **Mở dự án:**

   Mở file solution TiengAnh.sln bằng Visual Studio hoặc mở thư mục dự án bằng Visual Studio Code.

3. **Cấu hình MongoDB:**

   Cập nhật chuỗi kết nối MongoDB trong file appsettings.json:
   
   ```json
   "MongoDbSettings": {
     "ConnectionString": "mongodb://localhost:27017",
     "DatabaseName": "TiengAnh"
   }
   ```

4. **Khởi tạo dữ liệu:**

   Ứng dụng sử dụng `DataSeeder` để tạo dữ liệu mẫu khi khởi động lần đầu.

5. **Build và chạy dự án:**

   ```bash
   dotnet build
   dotnet run
   ```

6. **Truy cập ứng dụng:**

   Mở trình duyệt và truy cập địa chỉ: http://localhost:5000

## Cấu trúc chính

- **/Controllers**: Chứa các controller xử lý logic ứng dụng
- **/Models**: Định nghĩa cấu trúc dữ liệu
- **/Views**: Chứa giao diện người dùng
- **/Repositories**: Lớp truy cập dữ liệu
- **/Services**: Các dịch vụ như MongoDbService, DataSeeder
- **/wwwroot**: Tài nguyên tĩnh như CSS, JavaScript, images