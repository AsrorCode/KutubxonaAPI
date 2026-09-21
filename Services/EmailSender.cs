using System.Net;
using System.Net.Mail;

namespace KutubxonaAPI.Services;

/// <summary>Email yuborish abstraksiyasi (SMTP yoki log-fallback).</summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}

/// <summary>
/// SMTP sozlanmaganda — havolani logga yozadi. Dev'da bepul ishlaydi:
/// tasdiqlash/tiklash havolasini konsoldan olib sinash mumkin.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "📧 [EMAIL — SMTP sozlanmagan, log rejimi]\n  Kimga: {To}\n  Mavzu: {Subject}\n  Matn:\n{Body}",
            toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}

/// <summary>Haqiqiy SMTP orqali email (masalan Gmail: smtp.gmail.com:587 + app password).</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _config["Email:Smtp:Host"]!;
        var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
        var user = _config["Email:Smtp:User"];
        var pass = _config["Email:Smtp:Password"];
        var from = _config["Email:From"] ?? user ?? "no-reply@kutubxona.uz";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };
        using var msg = new MailMessage(from, toEmail, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(msg, ct);
        _logger.LogInformation("📧 Email yuborildi: {To}", toEmail);
    }
}
