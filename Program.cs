using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using KutubxonaAPI.Data;
using KutubxonaAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

// ============================================
// SERILOG — Startup uchun bootstrap logger
// ============================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 KutubxonaAPI ishga tushmoqda...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'ni asosiy loggerga aylantirish
    builder.Host.UseSerilog((context, services, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext();
    });

    // ============================================
    // SERVISLAR
    // ============================================

    // Controllers + OpenAPI (Scalar) — Enum'lar string sifatida
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi();

    // ============================================
    // FLUENTVALIDATION
    // ============================================
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    // ============================================
    // RESPONSE COMPRESSION (Gzip + Brotli)
    // ============================================
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
        {
            "application/json",
            "text/plain",
            "image/svg+xml"
        });
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

    // ============================================
    // IN-MEMORY CACHE + OUTPUT CACHE
    // ============================================
    builder.Services.AddMemoryCache();
    builder.Services.AddOutputCache(options =>
    {
        // Default: 60 sekund cache
        options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(60)));

        // Kategoriya kabi static data uchun — 5 daqiqa
        options.AddPolicy("static-5min", builder =>
            builder.Expire(TimeSpan.FromMinutes(5)));

        // Kitob ro'yxati — 30 sekund
        options.AddPolicy("books-30sec", builder =>
            builder.Expire(TimeSpan.FromSeconds(30))
                   .SetVaryByQuery("page", "pageSize", "category", "search"));
    });

    // Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection topilmadi");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // ============================================
    // HEALTH CHECKS
    // ============================================
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "ready" })
        .AddSqlServer(
            connectionString: connectionString,
            name: "sqlserver",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "ready" });

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
    // RATE LIMITING
    // ============================================
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = 429;

        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("general", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 10;
        });
    });

    // ============================================
    // CORS
    // ============================================
    var allowedOrigins = builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? new[] { "http://localhost:5000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });

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

    // Database migration
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        Log.Information("✅ Database migration bajarildi");
    }

    // ============================================
    // MIDDLEWARE PIPELINE
    // ============================================

    // 1. Global Exception Handler
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Response compression — birinchi bo'lib pipeline'da
    app.UseResponseCompression();

    // 2. Serilog request logging — har HTTP so'rov loglanadi
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.00}ms";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId",
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            }
        };
    });

    // 3. Scalar
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

    // 4. HTTPS
    app.UseHttpsRedirection();

    // 5. Static files
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // 6. CORS
    if (app.Environment.IsDevelopment())
        app.UseCors("Development");
    else
        app.UseCors("Production");

    // 7. Rate Limiter
    app.UseRateLimiter();

    // 8. Authentication + Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Output cache
    app.UseOutputCache();

    // ============================================
    // HEALTH CHECK ENDPOINTS
    // ============================================
    // Umumiy: /health — server ishlayaptimi
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Readiness: /health/ready — DB va boshqa dependency'lar tayyormi
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Liveness: /health/live — process tirikmi
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false, // Hech qanday check emas, faqat process javob berayaptimi
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    // 9. Controllers
    app.MapControllers();

    Log.Information("✅ KutubxonaAPI tayyor — {Env} muhitida", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 KutubxonaAPI ishga tushmadi");
}
finally
{
    Log.CloseAndFlush();
}
