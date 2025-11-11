using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EVServiceCenter.API.HostedServices;

/// <summary>
/// Warm-up dịch vụ cơ sở dữ liệu ngay khi API khởi động và giữ kết nối luôn “ấm”.
/// Điều này giúp tránh lần đăng nhập đầu tiên bị chậm do EF Core/SQL Server cold start.
/// </summary>
public class DatabaseWarmupHostedService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseWarmupHostedService> _logger;
    private Timer? _keepAliveTimer;
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromMinutes(5);

    public DatabaseWarmupHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseWarmupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EVDbContext>();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Mở kết nối sớm để khởi tạo pool và EF model.
            await dbContext.Database.OpenConnectionAsync(cancellationToken);

            // Chạy truy vấn nhẹ để kích hoạt cache & statistics.
            _ = await dbContext.Users.AsNoTracking().AnyAsync(cancellationToken);

            sw.Stop();
            _logger.LogInformation(
                "Database warm-up hoàn tất sau {ElapsedMs} ms - kết nối và plan đã sẵn sàng.",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database warm-up thất bại, tiếp tục khởi động bình thường.");
        }

        // Thiết lập timer keep-alive để hạn chế pool bị thu nhỏ hoặc DB ngủ đông.
        _keepAliveTimer = new Timer(
            async _ => await ExecuteKeepAliveAsync(),
            null,
            dueTime: KeepAliveInterval,
            period: KeepAliveInterval);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _keepAliveTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }

    private async Task ExecuteKeepAliveAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EVDbContext>();

            await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            _logger.LogDebug("Database keep-alive ping thành công {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Database keep-alive ping thất bại");
        }
    }

    public void Dispose()
    {
        _keepAliveTimer?.Dispose();
    }
}
