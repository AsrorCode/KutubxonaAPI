using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KutubxonaAPI.Common.Constants;
using KutubxonaAPI.Common.Extensions;
using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs.Auth;
using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.DTOs.Users;
using KutubxonaAPI.Models;
using KutubxonaAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Autentifikatsiya va foydalanuvchi hisobini boshqarish.
/// Register, login, refresh, logout, current user.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;
    private readonly KutubxonaAPI.Services.IEmailSender _email;

    public AuthController(
        AppDbContext context,
        IConfiguration config,
        ILogger<AuthController> logger,
        KutubxonaAPI.Services.IEmailSender email)
    {
        _context = context;
        _config = config;
        _logger = logger;
        _email = email;
    }

    // ============================================
    // REGISTER — Yangi foydalanuvchi
    // ============================================
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var emailLower = dto.Email.ToLower().Trim();
        var exists = await _context.Users.AnyAsync(u => u.Email == emailLower);
        if (exists)
            return BadRequest(new { message = "Bu email allaqachon ro'yxatdan o'tgan" });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Email = emailLower,
            PasswordHash = passwordHash,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi foydalanuvchi ro'yxatdan o'tdi: {Email}", user.Email);

        // Email tasdiqlash havolasini yuborish (yumshoq — bloklamaydi)
        await SendVerificationEmailAsync(user);

        // Access + Refresh token
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshToken(user.Id);

        return Ok(new AuthResponseDto
        {
            Message = "Ro'yxatdan muvaffaqiyatli o'tildi!",
            Token = accessToken,           // Backward compat
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = GetAccessTokenMinutes() * 60,
            User = user.ToDto()
        });
    }

    // ============================================
    // LOGIN — Kirish
    // ============================================
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var emailLower = dto.Email.ToLower().Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailLower);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Muvaffaqiyatsiz login urinishi: {Email}", emailLower);
            return Unauthorized(new { message = "Email yoki parol noto'g'ri" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Foydalanuvchi kirdi: {Email}", user.Email);

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshToken(user.Id);

        return Ok(new AuthResponseDto
        {
            Message = "Muvaffaqiyatli kirdingiz!",
            Token = accessToken,           // Backward compat
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = GetAccessTokenMinutes() * 60,
            User = user.ToDto()
        });
    }

    // ============================================
    // REFRESH — Yangi access token olish
    // ============================================
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest(new { message = "Refresh token yo'q" });

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Mavjud bo'lmagan refresh token: {Token}",
                dto.RefreshToken.Substring(0, Math.Min(10, dto.RefreshToken.Length)));
            return Unauthorized(new { message = "Refresh token noto'g'ri" });
        }

        if (!storedToken.IsActive)
        {
            _logger.LogWarning("Faol bo'lmagan refresh token ishlatilishga uring: UserId={UserId}",
                storedToken.UserId);
            return Unauthorized(new { message = "Refresh token muddati o'tgan yoki bekor qilingan" });
        }

        if (storedToken.User == null)
            return Unauthorized(new { message = "Foydalanuvchi topilmadi" });

        // Xavfsizlik: eski refresh tokenni bekor qilib, yangi yaratamiz (rotation)
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedReason = "Rotated";

        var newRefreshToken = await CreateRefreshToken(storedToken.UserId);
        var newAccessToken = GenerateAccessToken(storedToken.User);

        await _context.SaveChangesAsync();

        return Ok(new RefreshResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = GetAccessTokenMinutes() * 60
        });
    }

    // ============================================
    // LOGOUT — Refresh tokenni bekor qilish
    // ============================================
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return Ok(new { message = "Chiqildi" }); // Ideal — foydalanuvchi ochilgan tokensiz ham chiqadi

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (token != null && !token.IsRevoked)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Logout";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Foydalanuvchi chiqdi: UserId={UserId}", token.UserId);
        }

        return Ok(new { message = "Chiqildi" });
    }

    // ============================================
    // LOGOUT ALL — Barcha qurilmalardan chiqish
    // ============================================
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = "LogoutAll";
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Barcha qurilmalardan chiqildi: UserId={UserId}, {Count} ta", userId, tokens.Count);

        return Ok(new { message = $"{tokens.Count} ta seansdan chiqildi" });
    }

    // ============================================
    // ME — Joriy foydalanuvchi ma'lumoti
    // ============================================
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "Foydalanuvchi topilmadi" });

        return Ok(user.ToDto());
    }

    // ============================================
    // PUT ME — profil ma'lumotini tahrirlash (ism)
    // ============================================
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Foydalanuvchi topilmadi" });

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest(new { message = "Ism va familiya bo'sh bo'lmasligi kerak" });

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();

        // Avatar (ixtiyoriy). null bo'lsa tegilmaydi; hajm cheklovi.
        if (dto.AvatarUrl != null)
        {
            if (dto.AvatarUrl.Length > 3_000_000)
                return BadRequest(new { message = "Rasm juda katta (taxminan 2MB dan oshmasin)" });
            user.AvatarUrl = dto.AvatarUrl;
        }

        await _context.SaveChangesAsync();

        return Ok(user.ToDto());
    }

    // ============================================
    // EMAIL TASDIQLASH
    // ============================================
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct = default)
    {
        var t = await _context.UserTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token && x.Type == "verify", ct);

        if (t == null || t.IsUsed || t.ExpiresAt < DateTime.UtcNow || t.User == null)
            return BadRequest(new { message = "Havola noto'g'ri yoki muddati o'tgan" });

        t.User.IsEmailVerified = true;
        t.IsUsed = true;
        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Email tasdiqlandi" });
    }

    // ============================================
    // PAROLNI UNUTDIM — havola yuborish
    // ============================================
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct = default)
    {
        var email = (dto.Email ?? "").ToLower().Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Email mavjudligini oshkor qilmaymiz — har doim 200
        if (user != null)
        {
            var token = await CreateUserTokenAsync(user.Id, "reset", TimeSpan.FromHours(1), ct);
            var link = $"{Request.Scheme}://{Request.Host}/reset-password.html?token={token}";
            var body = $@"<p>Assalomu alaykum, {user.FirstName}!</p>
<p>Parolni tiklash uchun quyidagi havolani bosing (1 soat amal qiladi):</p>
<p><a href=""{link}"">{link}</a></p>
<p>Agar bu siz bo'lmasangiz — bu xatni e'tiborsiz qoldiring.</p>";
            await _email.SendAsync(user.Email, "Parolni tiklash — Kutubxona", body, ct);
        }

        return Ok(new { message = "Agar bu email ro'yxatda bo'lsa, tiklash havolasi yuborildi" });
    }

    // ============================================
    // PAROLNI TIKLASH
    // ============================================
    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Parol kamida 6 belgi bo'lishi kerak" });

        var t = await _context.UserTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == dto.Token && x.Type == "reset", ct);

        if (t == null || t.IsUsed || t.ExpiresAt < DateTime.UtcNow || t.User == null)
            return BadRequest(new { message = "Havola noto'g'ri yoki muddati o'tgan" });

        t.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        t.IsUsed = true;

        // Xavfsizlik — barcha eski refresh tokenlarni bekor qilish
        var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == t.UserId && !rt.IsRevoked).ToListAsync(ct);
        foreach (var rt in tokens) { rt.IsRevoked = true; rt.RevokedAt = DateTime.UtcNow; rt.RevokedReason = "Parol tiklandi"; }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Parol tiklandi: UserId={UserId}", t.UserId);
        return Ok(new { message = "Parol yangilandi. Endi kirishingiz mumkin." });
    }

    // Email tasdiqlash havolasini yuborish (yordamchi)
    private async Task SendVerificationEmailAsync(User user)
    {
        var token = await CreateUserTokenAsync(user.Id, "verify", TimeSpan.FromDays(3));
        var link = $"{Request.Scheme}://{Request.Host}/verify-email.html?token={token}";
        var body = $@"<p>Assalomu alaykum, {user.FirstName}!</p>
<p>Kutubxonaga xush kelibsiz. Emailingizni tasdiqlash uchun havolani bosing:</p>
<p><a href=""{link}"">{link}</a></p>";
        await _email.SendAsync(user.Email, "Emailni tasdiqlang — Kutubxona", body);
    }

    // Xavfsiz token yaratish
    private async Task<string> CreateUserTokenAsync(int userId, string type, TimeSpan lifetime, CancellationToken ct = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
        _context.UserTokens.Add(new UserToken
        {
            UserId = userId,
            Token = token,
            Type = type,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);
        return token;
    }

    // ============================================
    // YORDAMCHI METODLAR
    // ============================================

    /// <summary>Access token muddati (daqiqa). Konfiguratsiyadan yoki default.</summary>
    private int GetAccessTokenMinutes()
        => int.TryParse(_config["Jwt:AccessMinutes"], out var m) ? m : AuthConstants.AccessTokenMinutes;

    /// <summary>Refresh token muddati (kun). Konfiguratsiyadan yoki default.</summary>
    private int GetRefreshTokenDays()
        => int.TryParse(_config["Jwt:RefreshDays"], out var d) ? d : AuthConstants.RefreshTokenDays;

    private string GenerateAccessToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key topilmadi!");
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> CreateRefreshToken(int userId)
    {
        // 64 byte = 512 bit crypto-random
        var randomBytes = new byte[AuthConstants.RefreshTokenBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        var token = Convert.ToBase64String(randomBytes)
            .Replace("+", "-").Replace("/", "_").Replace("=", ""); // URL-safe

        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenDays()),
            CreatedAt = DateTime.UtcNow,
            UserAgent = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}

// ============================================
// DTO SINFLARI
// ============================================

public class RegisterDto
{
    [Required(ErrorMessage = "Email kerak")]
    [EmailAddress(ErrorMessage = "Email format noto'g'ri")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parol kerak")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Parol 8-100 belgi bo'lishi kerak")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "Parolda kamida 1 katta harf, 1 kichik harf va 1 raqam bo'lishi kerak"
    )]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ism kerak")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ism 2-50 belgi")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Familiya kerak")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Familiya 2-50 belgi")]
    public string LastName { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Email kerak")]
    [EmailAddress(ErrorMessage = "Email format noto'g'ri")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parol kerak")]
    public string Password { get; set; } = string.Empty;
}

public class RefreshDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Ism kerak")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ism 2-50 belgi")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Familiya kerak")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Familiya 2-50 belgi")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Profil rasmi (base64 yoki URL). null = tegilmaydi.</summary>
    public string? AvatarUrl { get; set; }
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email kerak")]
    [EmailAddress(ErrorMessage = "Email format noto'g'ri")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yangi parol kerak")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Parol kamida 6 belgi")]
    public string NewPassword { get; set; } = string.Empty;
}
