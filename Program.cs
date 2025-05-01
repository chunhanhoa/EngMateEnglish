using TiengAnh.Models;
using TiengAnh.Repositories;
using TiengAnh.Services;
using TiengAnh.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using MongoDB.Bson.Serialization.Conventions;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký MongoDB services - sửa tham chiếu mơ hồ bằng cách chỉ định đầy đủ namespace
builder.Services.Configure<TiengAnh.Services.MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDbService>();

// Đăng ký các repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<GrammarRepository>();
builder.Services.AddScoped<VocabularyRepository>();
builder.Services.AddScoped<TestRepository>();
builder.Services.AddScoped<TopicRepository>();
builder.Services.AddScoped<ExerciseRepository>();
builder.Services.AddScoped<ProgressRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

// Đăng ký DataSeeder
builder.Services.AddScoped<DataSeeder>();

// Đảm bảo có cấu hình authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    // Thêm các cấu hình khác nếu cần
});

// Thêm dòng này vào cấu hình services để ghi nhớ rằng đã xử lý các cảnh báo nullable
builder.Services.SuppressNullableWarnings();

// Thêm cấu hình cho Kestrel server để xử lý request tốt hơn
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});

// Tăng thời gian timeout cho HTTP request
builder.Services.AddHttpClient(string.Empty, client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});

// Cấu hình bổ sung để xử lý lỗi IFeatureCollection disposed
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

var app = builder.Build();

// Thêm cấu hình cho MongoDB Serialization
var pack = new ConventionPack
{
    new IgnoreExtraElementsConvention(true)
};
ConventionRegistry.Register("IgnoreExtraElements", pack, t => true);

// Thêm code để seed dữ liệu khi ứng dụng khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting application initialization");
        
        var dataSeeder = services.GetRequiredService<DataSeeder>();
        logger.LogInformation("Running data seeder");
        await dataSeeder.SeedDataAsync();
        
        // Kiểm tra xem dữ liệu đã được seed chưa
        var topicRepo = services.GetRequiredService<TopicRepository>();
        var hasTopics = await topicRepo.HasDataAsync();
        logger.LogInformation($"Topics data exists: {hasTopics}");

        // Thêm kiểm tra Vocabularies
        var vocabRepo = services.GetRequiredService<VocabularyRepository>();
        var topicVocabs = await vocabRepo.GetByTopicIdAsync(1); 
        logger.LogInformation($"Vocabularies for Topic 1: {topicVocabs.Count}");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during application startup");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Kiểm tra null trước khi truy cập thuộc tính
    if (app.Environment?.WebRootPath != null)
    {
        // Code sử dụng app.Environment.WebRootPath
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cấu hình xử lý file tĩnh
app.UseStaticFiles();

// Chuyển hướng index.html đến trang chủ
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value == "/index.html")
    {
        context.Response.Redirect("/");
        return;
    }
    await next();
});

// Chặn tất cả các yêu cầu đến Swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger") || 
        context.Request.Path == "/index.html" && context.Request.QueryString.Value.Contains("swagger"))
    {
        context.Response.Redirect("/");
        return;
    }
    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Thêm dòng này vào cấu hình middleware
app.SuppressNullableWarnings();

// Cấu hình routes cho MVC - đảm bảo route mặc định đúng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Đảm bảo rằng cấu hình này chỉ chạy khi không có các route khác xử lý request
app.MapFallbackToController("Index", "Home");

app.Run();
