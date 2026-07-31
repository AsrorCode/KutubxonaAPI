using System.Text;
using System.Threading.RateLimiting;
using KutubxonaAPI.Data;
using KutubxonaAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVISLAR
// ============================================

// Controllers + OpenAPI (Scalar)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database — EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key topilmadi!");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ============================================
// RATE LIMITING — Bot hujumidan himoya
// ============================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    // Login/Register uchun QATTIQ limit — 5 ta so'rov/daqiqa
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Umumiy API uchun limit — 100 ta so'rov/daqiqa
    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
    });
});

// ============================================
// CORS — Faqat ruxsat berilgan domenlar
// ============================================
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5000" };

builder.Services.AddCors(options =>
{
    // Development uchun — hammaga ochiq
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Production uchun — faqat ruxsat etilganlar
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================
// APP QURISH
// ============================================
var app = builder.Build();

// Database avtomatik migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// ============================================
// MIDDLEWARE PIPELINE — TARTIB MUHIM!
// ============================================

// 1. EN AVVAL — Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Scalar (development'da)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("KutubxonaAPI")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
    });
}

// 3. HTTPS
app.UseHttpsRedirection();

// 4. Static files (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// 5. CORS — Environment'ga qarab
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

// 6. Rate Limiter
app.UseRateLimiter();

// 7. Authentication (Authorization dan OLDIN!)
app.UseAuthentication();
app.UseAuthorization();

// 8. Controllers
app.MapControllers();

app.Run();