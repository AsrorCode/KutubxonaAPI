using KutubxonaAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Services;

/// <summary>
/// Fon xizmati — eskirgan yozuvlarni muntazam tozalaydi (har 24 soatda):
///  • muddati o'tgan / bekor qilingan refresh tokenlar,
///  • ishlatilgan yoki muddati o'tgan bir martalik tokenlar,
///  • 30 kundan eski bildirishnomalar.
/// Jadvallar cheksiz o'smasligi uchun.
/// </summary>
public class CleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupHostedService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public CleanupHostedService(IServiceScopeFactory scopeFactory, ILogger<CleanupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Startdan 1 daqiqa keyin birinchi tozalash (ishga tushishni sekinlashtirmaslik uchun)
        try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tozalash xizmatida xato");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var tokenCutoff = now.AddDays(-7);        // revoke/expire bo'lgandan 7 kun o'tgach
        var notifCutoff = now.AddDays(-30);       // 30 kundan eski bildirishnoma

        var refreshDeleted = await db.RefreshTokens
            .Where(t => (t.IsRevoked && t.RevokedAt != null && t.RevokedAt < tokenCutoff)
                        || t.ExpiresAt < tokenCutoff)
            .ExecuteDeleteAsync(ct);

        var userTokenDeleted = await db.UserTokens
            .Where(t => t.IsUsed || t.ExpiresAt < now)
            .ExecuteDeleteAsync(ct);

        var notifDeleted = await db.Notifications
            .Where(n => n.CreatedAt < notifCutoff)
            .ExecuteDeleteAsync(ct);

        if (refreshDeleted + userTokenDeleted + notifDeleted > 0)
        {
            _logger.LogInformation(
                "🧹 Tozalash: refresh={Refresh}, userToken={UserToken}, notif={Notif}",
                refreshDeleted, userTokenDeleted, notifDeleted);
        }
    }
}
