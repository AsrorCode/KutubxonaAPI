using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Foydalanuvchi autentifikatsiyasi: Register, Login, Me
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration config, ILogger<AuthController> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    // ==========================================
    // POST /api/auth/register - Ro'yxatdan o'tish
    // ==========================================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Email mavjudligini tekshirish
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (emailExists)
        {
            return BadRequest(new { message = "Bu email allaqachon ro'yxatdan o'tgan" });
        }

        // Parolni hash qilish (BCrypt)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Yangi User yaratish
        var user = new User
        {
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Role = "User",  // Default rol
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi foydalanuvchi ro'yxatdan o'tdi: {Email}", user.Email);

        // JWT token yaratish
        var token = GenerateJwtToken(user);

        return Created($"/api/auth/me", new
        {
            message = "Muvaffaqiyatli ro'yxatdan o'tdingiz!",
            token,
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role,
                user.FullName
            }
        });
    }

    // ==========================================
    // POST /api/auth/login - Tizimga kirish
    // ==========================================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Foydalanuvchini topish
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (user == null)
        {
            return Unauthorized(new { message = "Email yoki parol noto'g'ri" });
        }

        // Parolni tekshirish
        bool passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!passwordOk)
        {
            return Unauthorized(new { message = "Email yoki parol noto'g'ri" });
        }

        // Oxirgi kirish vaqtini yangilash
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Foydalanuvchi kirdi: {Email}", user.Email);

        // JWT token yaratish
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
                user.Role,
                user.FullName
            }
        });
    }

    // ==========================================
    // GET /api/auth/me - Joriy foydalanuvchi haqida
    // ==========================================
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // Authorization header'dan token olish
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { message = "Token yo'q" });
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            // Token'ni o'qish
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null)
                return Unauthorized(new { message = "Token noto'g'ri" });

            int userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "Foydalanuvchi topilmadi" });

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role,
                user.FullName,
                user.CreatedAt,
                user.LastLoginAt
            });
        }
        catch
        {
            return Unauthorized(new { message = "Token noto'g'ri yoki muddati o'tgan" });
        }
    }

    // ==========================================
    // YORDAMCHI: JWT token yaratish
    // ==========================================
    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]!;
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];
        var expireDays = int.Parse(_config["Jwt:ExpireDays"] ?? "7");

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expireDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ==========================================
// DTO klasslar
// ==========================================

public class RegisterDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email kiritilishi shart")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "To'g'ri email kiriting")]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Parol kiritilishi shart")]
    [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Parol kamida 6 ta belgi bo'lishi kerak")]
    public string Password { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ism kiritilishi shart")]
    public string FirstName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Familiya kiritilishi shart")]
    public string LastName { get; set; } = string.Empty;
}

public class LoginDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Password { get; set; } = string.Empty;
}