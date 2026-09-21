using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Rasm yuklash — base64'ni fayl sifatida wwwroot/uploads'ga saqlaydi va URL qaytaradi.
/// Shu tufayli baza yengil qoladi (base64 o'rniga qisqa URL).
/// </summary>
[ApiController]
[Route("api/uploads")]
[Authorize]
[Produces("application/json")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadsController> _logger;

    private static readonly Dictionary<string, string> AllowedTypes = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif"
    };

    public UploadsController(IWebHostEnvironment env, ILogger<UploadsController> logger)
    {
        _env = env;
        _logger = logger;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage([FromBody] UploadImageDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.DataUrl) || !dto.DataUrl.StartsWith("data:"))
            return BadRequest(new { message = "Rasm ma'lumoti noto'g'ri" });

        var comma = dto.DataUrl.IndexOf(',');
        if (comma < 6) return BadRequest(new { message = "Format noto'g'ri" });

        // "data:image/png;base64" -> "image/png"
        var meta = dto.DataUrl.Substring(5, comma - 5);
        var mime = meta.Split(';')[0].Trim().ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(mime, out var ext))
            return BadRequest(new { message = "Faqat JPG, PNG, WEBP, GIF ruxsat etiladi" });

        byte[] bytes;
        try { bytes = Convert.FromBase64String(dto.DataUrl[(comma + 1)..]); }
        catch { return BadRequest(new { message = "Rasm ma'lumoti buzuq (base64)" }); }

        if (bytes.Length == 0) return BadRequest(new { message = "Bo'sh rasm" });
        if (bytes.Length > 3 * 1024 * 1024)
            return BadRequest(new { message = "Rasm juda katta (max 3MB)" });

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var dir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(dir, fileName), bytes, ct);

        var url = $"/uploads/{fileName}";
        _logger.LogInformation("Rasm yuklandi: {Url} ({Size} bayt)", url, bytes.Length);
        return Ok(new { url });
    }
}

public class UploadImageDto
{
    public string DataUrl { get; set; } = string.Empty;
}
