using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TiengAnh.Services
{
    public class KeepAliveService : BackgroundService
    {
        private readonly ILogger<KeepAliveService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _appUrl;

        public KeepAliveService(ILogger<KeepAliveService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _appUrl = configuration["RENDER_EXTERNAL_URL"] ?? "https://engmateenglish.onrender.com/";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Chỉ chạy trong production
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    using var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    
                    // Ping mỗi 10 phút để keep alive
                    await httpClient.GetAsync($"{_appUrl}/ping", stoppingToken);
                    _logger.LogInformation("Keep-alive ping sent successfully");
                    
                    // Chờ 10 phút
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Keep-alive ping failed");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
