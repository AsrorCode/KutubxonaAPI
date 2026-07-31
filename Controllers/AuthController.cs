using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
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
        // Model validatsiya
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Email band emasligini tekshirish
        var emailLower = dto.Email.ToLower().Trim();
        var exists = await _context.Users.AnyAsync(u => u.Email == emailLower);
        if (exists)
            return BadRequest(new { message = "Bu email allaqachon ro'yxatdan o'tgan" });

        // Parolni BCrypt bilan hash qilish
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Foydalanuvchi yaratish
        var user = new User
        {
            Email = emailLower,
            PasswordHash = passwordHash,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi foydalanuvchi ro'yxatdan o'tdi: {Email}", user.Email);

        // JWT token yaratish
        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Ro'yxatdan muvaffaqiyatli o'tildi!",
            token,
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role
            }
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

        // XAVFSIZLIK: bir xil xabar — email/parolni aniqlab bo'lmasin
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Muvaffaqiyatsiz login urinishi: {Email}", emailLower);
            return Unauthorized(new { message = "Email yoki parol noto'g'ri" });
        }

        // Oxirgi kirish vaqtini yangilash
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Foydalanuvchi kirdi: {Email}", user.Email);

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Muvaffaqiyatli kirdingiz!",
            token,
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role
            }
        });
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

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.CreatedAt,
                u.LastLoginAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Foydalanuvchi topilmadi" });

        return Ok(user);
    }

    // ============================================
    // JWT TOKEN YARATISH — Yordamchi metod
    // ============================================
    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key topilmadi!");
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];
        var expireDays = int.Parse(_config["Jwt:ExpireDays"] ?? "7");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expireDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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