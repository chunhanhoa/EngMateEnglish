using TiengAnh.Models;
using TiengAnh.Repositories;
using TiengAnh.Services;
using TiengAnh.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using MongoDB.Bson.Serialization.Conventions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using TiengAnh.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register MongoDB services
builder.Services.Configure<TiengAnh.Services.MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbService>();

// Register repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<GrammarRepository>();
builder.Services.AddScoped<VocabularyRepository>();
builder.Services.AddScoped<TestRepository>();
builder.Services.AddScoped<UserTestRepository>(); // Add this line

// Fix registration of ITestRepository - ensure TestRepository is registered first, then register the interface
builder.Services.AddScoped<ITestRepository>(sp => sp.GetRequiredService<TestRepository>());

builder.Services.AddScoped<TopicRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<ExerciseRepository>(provider =>
{
    var mongoDbService = provider.GetRequiredService<MongoDbService>();
    var hostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
    return new ExerciseRepository(mongoDbService, hostEnvironment.ContentRootPath);
});

// Register DataSeeder and DataImportService
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<DataImportService>();

// Add User Statistics Service
builder.Services.AddScoped<UserStatisticsService>();

// Configure authentication
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
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    
    // Fix correlation issues by configuring events
    options.Events.OnTicketReceived = ctx =>
    {
        Console.WriteLine("Google authentication ticket received successfully");
        return Task.CompletedTask;
    };

    options.Events.OnRemoteFailure = ctx =>
    {
        Console.WriteLine($"Google authentication failed: {ctx.Failure?.Message}");
        ctx.Response.Redirect("/Account/Login?error=Google_auth_failed");
        ctx.HandleResponse();
        return Task.CompletedTask;
    };

    // Override the default CORS validation to fix correlation issues
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    
    options.SaveTokens = true;
});

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Kestrel server
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});

// Configure HTTP client timeout
builder.Services.AddHttpClient(string.Empty, client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});

// Configure IIS to allow synchronous IO
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

var app = builder.Build();

// Configure MongoDB serialization
var pack = new ConventionPack
{
    new IgnoreExtraElementsConvention(true)
};
ConventionRegistry.Register("IgnoreExtraElements", pack, t => true);

// Seed data during startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting application initialization");

        // Run DataSeeder
        var dataSeeder = services.GetRequiredService<DataSeeder>();
        logger.LogInformation("Running data seeder");
        await dataSeeder.SeedAllDataAsync(); // Changed from SeedDataAsync to SeedAllDataAsync

        // Check seeded data
        var topicRepo = services.GetRequiredService<TopicRepository>();
        var hasTopics = await topicRepo.HasDataAsync();
        logger.LogInformation($"Topics data exists: {hasTopics}");

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

// Import exercises during startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dataImportService = services.GetRequiredService<DataImportService>();
            string exercisesJsonPath = Path.Combine(app.Environment.ContentRootPath, "json", "exercise.json");
            if (File.Exists(exercisesJsonPath))
            {
                await dataImportService.ImportExercisesFromJson(exercisesJsonPath);
                Console.WriteLine($"Imported exercises from {exercisesJsonPath}");
            }
            else
            {
                Console.WriteLine($"Exercise JSON file not found at: {exercisesJsonPath}");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while importing exercises");
        }
    }
}

// Seed exercises from JSON
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var exerciseRepository = services.GetRequiredService<ExerciseRepository>();
        await exerciseRepository.SeedExercisesFromJsonAsync();
        Console.WriteLine("Seeded exercises from JSON");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding exercises");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add debug middleware to help with OAuth troubleshooting
app.UseMiddleware<OAuthDebugMiddleware>();

// Add session middleware
app.UseSession();

// Redirect index.html to home
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value == "/index.html")
    {
        context.Response.Redirect("/");
        return;
    }
    await next();
});

// Block Swagger requests
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

// Configure MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Fallback route
app.MapFallbackToController("Index", "Home");

app.Run();