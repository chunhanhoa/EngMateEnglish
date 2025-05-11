# DoAnCoSo (EngMate)

## Giới thiệu

**EngMate** là một ứng dụng web học tiếng Anh toàn diện, được phát triển theo kiến trúc MVC. Ứng dụng cung cấp nhiều tính năng giúp người học tiếng Anh cải thiện kỹ năng ngôn ngữ một cách hiệu quả thông qua các bài học từ vựng, ngữ pháp, bài tập thực hành và kiểm tra.

Dự án được xây dựng sử dụng **ASP.NET Core MVC** với ngôn ngữ **C#**, tích hợp cơ sở dữ liệu **MongoDB**, cùng giao diện người dùng được thiết kế bằng **HTML**, **CSS**, **Bootstrap** và **JavaScript**.

## Tính năng chính

### Học từ vựng
- **Danh sách từ vựng phong phú**: Từ vựng được phân loại theo chủ đề và trình độ (A1-C1)
- **Chi tiết từ vựng đầy đủ**: Hiển thị nghĩa, ví dụ, phát âm và hình ảnh minh họa
- **Hình ảnh thông minh**: Tự động ẩn hình ảnh cho các loại từ trừu tượng (trạng từ, giới từ, v.v.)
- **Phát âm tự động**: Tính năng text-to-speech cho việc luyện phát âm
- **Từ điển tích hợp**: Tự động đề xuất nghĩa, ví dụ và phân loại từ khi thêm từ mới

### Học ngữ pháp
- **Bài học có cấu trúc**: Bài học ngữ pháp từ cơ bản đến nâng cao (A1-C1)
- **Nội dung phong phú**: Bài học bao gồm giải thích, ví dụ và bài tập thực hành
- **Hỗ trợ video**: Tích hợp video YouTube cho mỗi điểm ngữ pháp
- **Quản lý ví dụ**: Hỗ trợ thêm nhiều ví dụ minh họa cho khái niệm ngữ pháp

### Bài tập và kiểm tra
- **Đa dạng loại bài tập**: Trắc nghiệm, điền khuyết, sắp xếp từ, nối từ
- **Kiểm tra đánh giá**: Các bài kiểm tra để đánh giá năng lực học viên
- **Phản hồi tức thì**: Hiển thị kết quả và giải thích ngay sau khi hoàn thành

### Tính năng người dùng
- **Yêu thích**: Đánh dấu và lưu từ vựng, ngữ pháp yêu thích để học lại
- **Theo dõi tiến độ**: Hệ thống theo dõi chi tiết quá trình học tập
- **Giao diện đáp ứng**: Tương thích với nhiều thiết bị (máy tính, máy tính bảng, điện thoại)

## Cải tiến mới

### Quản lý từ vựng
- **Hình ảnh tùy chọn**: Cho phép tải lên hình ảnh tùy chọn cho từ vựng, đặc biệt là với các loại từ trừu tượng
- **Hiển thị thông minh hình ảnh**: Không hiển thị hình ảnh cho các loại từ:
  - Trạng từ (adv)
  - Giới từ (prep)
  - Liên từ (conj)
  - Hạn định từ (det)
  - Thán từ (interj)
- **Hướng dẫn rõ ràng**: Bổ sung văn bản hướng dẫn chỉ rõ trường hình ảnh là tùy chọn

### Tính năng tự động đề xuất
- **Tra từ tự động**: Nâng cao tính năng tra cứu từ vựng với khả năng tự động đề xuất:
  - Bản dịch tiếng Việt
  - Câu ví dụ
  - Phân loại từ loại
  - Cấp độ khó phù hợp
  - Phân loại chủ đề liên quan

### Cải tiến biểu mẫu
- **Sửa lỗi xác thực**: Khắc phục vấn đề với các trường tùy chọn
- **Xử lý biểu mẫu thông minh**: Ngăn chặn thông báo lỗi không cần thiết
- **Phản hồi người dùng**: Cải thiện thông báo khi gửi biểu mẫu lỗi

### Tạo nội dung ngữ pháp
- **Hỗ trợ nhiều ví dụ**: Cho phép thêm nhiều ví dụ trong bài học ngữ pháp
- **Giao diện động**: Triển khai giao diện thêm/xóa ví dụ linh hoạt
- **Chức năng xem trước**: Thêm tính năng xem trước cho người tạo nội dung

### Cải thiện hiệu suất
- **Tối ưu hóa hình ảnh**: Cải thiện tải và hiển thị hình ảnh
- **Debouncing API**: Thêm debouncing cho các cuộc gọi API để cải thiện hiệu suất
- **Cải thiện trải nghiệm mobile**: Nâng cao khả năng hiển thị trên thiết bị di động

## Công nghệ sử dụng

- **ASP.NET Core MVC**: Framework phát triển web
- **C#**: Ngôn ngữ lập trình backend
- **MongoDB**: Cơ sở dữ liệu NoSQL
- **Bootstrap 5**: Framework CSS cho giao diện đáp ứng
- **Font Awesome**: Thư viện biểu tượng
- **JavaScript/jQuery**: Tương tác phía client
- **AJAX**: Giao tiếp bất đồng bộ với server
- **Web Speech API**: Tích hợp phát âm cho từ vựng

## API tích hợp
- **Dictionary API**: Tra cứu định nghĩa và ví dụ cho từ vựng tiếng Anh
- **Translation API**: Dịch từ tiếng Anh sang tiếng Việt
- **YouTube API**: Nhúng video hướng dẫn cho bài học ngữ pháp

## Kiến trúc dự án

- **Models**: Định nghĩa cấu trúc dữ liệu (GrammarModel, VocabularyModel, TestModel,...)
- **Views**: Giao diện người dùng
- **Controllers**: Xử lý logic ứng dụng
- **Repositories**: Truy cập và thao tác dữ liệu
- **Services**: Xử lý các dịch vụ như kết nối MongoDB, seeding dữ liệu
- **Middleware**: Xử lý yêu cầu HTTP như xác thực, xử lý lỗi

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

4. **Cấu hình API (tùy chọn):**

   ```json
   "ExternalServices": {
     "DictionaryApiUrl": "https://api.dictionaryapi.dev/api/v2/entries/en",
     "TranslationApiUrl": "https://api.mymemory.translated.net/get"
   }
   ```

5. **Khởi tạo dữ liệu:**

   Ứng dụng sử dụng `DataSeeder` để tạo dữ liệu mẫu khi khởi động lần đầu.

6. **Build và chạy dự án:**

   ```bash
   dotnet build
   dotnet run
   ```

7. **Truy cập ứng dụng:**

   Mở trình duyệt và truy cập địa chỉ: http://localhost:5000

## Cách sử dụng

### Thêm từ vựng mới
1. Truy cập vào "Nội dung > Thêm từ vựng"
2. Nhập từ tiếng Anh - hệ thống sẽ tự động đề xuất các thông tin liên quan
3. Xem và điều chỉnh thông tin được đề xuất nếu cần
4. Thêm hình ảnh (tùy chọn, đặc biệt với các khái niệm trừu tượng)
5. Lưu để thêm từ vựng mới vào hệ thống

### Tạo bài học ngữ pháp
1. Truy cập "Nội dung > Thêm bài học ngữ pháp"
2. Nhập tiêu đề và mô tả của bài học
3. Viết nội dung chi tiết bằng Markdown để định dạng
4. Thêm ví dụ để minh họa khái niệm ngữ pháp
5. Tùy chọn thêm hình ảnh hoặc liên kết video
6. Lưu bài học ngữ pháp

## Cấu trúc thư mục

- **/Controllers**: Chứa các controller xử lý logic ứng dụng
- **/Models**: Định nghĩa cấu trúc dữ liệu
- **/Views**: Chứa giao diện người dùng
- **/Repositories**: Lớp truy cập dữ liệu
- **/Services**: Các dịch vụ như MongoDbService, DataSeeder
- **/wwwroot**: Tài nguyên tĩnh như CSS, JavaScript, images
- **/Middleware**: Các middleware tùy chỉnh
- **/Helpers**: Các lớp hỗ trợ và tiện ích
