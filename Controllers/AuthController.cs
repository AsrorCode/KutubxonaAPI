using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext context,
        IConfiguration config,
        ILogger<AuthController> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "Foydalanuvchi topilmadi" });

        return Ok(user.ToDto());
    }

    // ============================================
    // YORDAMCHI METODLAR
    // ============================================

    private int GetAccessTokenMinutes()
    {
        return int.Parse(_config["Jwt:AccessMinutes"] ?? "15");
    }

    private int GetRefreshTokenDays()
    {
        return int.Parse(_config["Jwt:RefreshDays"] ?? "7");
    }

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
        var randomBytes = new byte[64];
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
