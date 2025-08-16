using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace TiengAnh.Services
{
    public class KeepAliveService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<KeepAliveService> _logger;
        private readonly string _targetUrl;
        private static readonly TimeSpan FastInterval = TimeSpan.FromMinutes(8);
        private static readonly TimeSpan SlowInterval = TimeSpan.FromMinutes(14);

        public KeepAliveService(IHttpClientFactory httpClientFactory, ILogger<KeepAliveService> logger, IConfiguration cfg)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _targetUrl =
                cfg["SelfPing:Url"] ??
                Environment.GetEnvironmentVariable("SELF_PING_URL") ??
                Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL") ??
                "https://engmateenglish.onrender.com";
            if (!string.IsNullOrWhiteSpace(_targetUrl) && !_targetUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                _targetUrl = "https://" + _targetUrl.Trim().TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(_targetUrl))
                _targetUrl = _targetUrl.TrimEnd('/') + "/";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
                return;

            if (string.IsNullOrWhiteSpace(_targetUrl))
            {
                _logger.LogInformation("KeepAliveService: Không có URL để ping. Tắt service.");
                return;
            }

            _logger.LogInformation("KeepAliveService: Bắt đầu ping {url} để giữ server thức", _targetUrl);

            try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); } catch { }

            int consecutiveSuccesses = 0;
            int consecutiveFailures = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(15);

                    using var req = new HttpRequestMessage(HttpMethod.Head, _targetUrl);
                    req.Headers.Add("User-Agent", "KeepAlive/1.0");

                    var resp = await client.SendAsync(req, stoppingToken);
                    sw.Stop();

                    if (resp.IsSuccessStatusCode)
                    {
                        consecutiveSuccesses++;
                        consecutiveFailures = 0;
                        if (consecutiveSuccesses <= 3 || consecutiveSuccesses % 10 == 0)
                            _logger.LogInformation("✅ Ping thành công #{count} - {status} ({ms}ms)",
                                consecutiveSuccesses, (int)resp.StatusCode, sw.ElapsedMilliseconds);
                        else
                            _logger.LogDebug("✅ Ping OK #{count} - {ms}ms", consecutiveSuccesses, sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        consecutiveFailures++;
                        consecutiveSuccesses = 0;
                        _logger.LogWarning("⚠️ Ping lỗi HTTP {status} ({ms}ms) - lần thứ {failures}",
                            (int)resp.StatusCode, sw.ElapsedMilliseconds, consecutiveFailures);
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    consecutiveFailures++;
                    consecutiveSuccesses = 0;
                    _logger.LogWarning("⏰ Ping timeout sau {ms}ms - lần thứ {failures}", sw.ElapsedMilliseconds, consecutiveFailures);
                }
                catch (Exception ex)
                {
                    consecutiveFailures++;
                    consecutiveSuccesses = 0;
                    _logger.LogWarning("❌ Ping thất bại: {error} - lần thứ {failures}", ex.Message, consecutiveFailures);
                }

                var interval = (consecutiveSuccesses >= 5 && consecutiveFailures == 0) ? SlowInterval : FastInterval;
                if (consecutiveFailures >= 3)
                {
                    interval = TimeSpan.FromMinutes(5);
                    _logger.LogWarning("🔄 Server có vấn đề, tăng tần suất ping lên mỗi 5 phút");
                }

                try { await Task.Delay(interval, stoppingToken); }
                catch { break; }
            }

            _logger.LogInformation("KeepAliveService đã dừng");
        }
    }
}
