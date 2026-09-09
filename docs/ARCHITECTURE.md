# 🏛️ Arxitektura

Bu hujjatda **KutubxonaAPI** loyihasining arxitekturaviy qarorlari, papka
tuzilmasi va ma'lumot oqimi tushuntiriladi.

## 📐 Umumiy strukturasi

```
┌─────────────┐    HTTPS + JWT     ┌──────────────┐
│  Browser    │ ─────────────────► │  ASP.NET     │
│  (HTML/JS)  │                    │  Core API    │
└─────────────┘ ◄───────────────── │  (.NET 10)   │
                    JSON           └──────┬───────┘
                                          │ EF Core
                                          ▼
                                   ┌──────────────┐
                                   │ SQL Server   │
                                   │ (LocalDB)    │
                                   └──────────────┘
```

## 📁 Papka tuzilmasi

```
KutubxonaAPI/
├── Controllers/       # HTTP endpoint'lar (thin)
├── Common/            # Butun loyiha uchun umumiy
│   ├── Constants/     # AppConstants, AuthConstants, OrderConstants
│   └── Extensions/    # ClaimsPrincipalExtensions
├── Data/              # AppDbContext + Global Query Filters
├── DTOs/              # Data Transfer Objects
│   ├── Auth/          # AuthResponseDto, RefreshResponseDto
│   ├── Books/         # BookResponseDto, BookCreateDto...
│   ├── Comments/      # CommentResponseDto, CommentsListDto
│   ├── Mapping/       # Entity ↔ DTO extensions
│   ├── Orders/        # OrderSummaryDto, OrderDetailDto...
│   ├── SaleBooks/     # SaleBookResponseDto, SaleBookCreateDto
│   └── Users/         # UserResponseDto
├── Exceptions/        # AppException va bolalari (NotFound, Conflict...)
├── Middleware/        # GlobalExceptionMiddleware
├── Migrations/        # EF Core migratsiya fayllari
├── Models/            # Entitylar
│   ├── Enums/         # OrderStatus, UserRole
│   └── ...            # Book, User, Order va h.k.
├── Validators/        # FluentValidation qoidalari
├── wwwroot/           # Statik fayllar (frontend)
├── .github/           # GitHub Actions CI/CD
└── docs/              # Loyiha hujjatlari
```

## 🔄 So'rov oqimi (Request Flow)

```
Client
  │
  ▼
[GlobalExceptionMiddleware]      ← barcha xatolarni tutadi
  │
  ▼
[Serilog Request Logging]        ← har so'rov loglanadi
  │
  ▼
[UseAuthentication]              ← JWT validatsiya
  │
  ▼
[UseAuthorization]               ← Role tekshirish
  │
  ▼
[Rate Limiter]                   ← 5/daq (auth), 100/daq (umumiy)
  │
  ▼
[Controller Action]
  │  - FluentValidation avtomatik ishlaydi
  │  - DTO ↔ Entity mapping
  │  - Business logic
  │
  ▼
[Entity Framework Core]
  │  - Global Query Filter (Soft Delete)
  │  - RowVersion concurrency check
  │
  ▼
[SQL Server]
```

## 🎯 Asosiy dizayn qarorlar

### 1. DTO Layer
- **Models hech qachon to'g'ridan-to'g'ri qaytarilmaydi** (PasswordHash xavfi)
- Har entity uchun Create/Update/Response DTO
- Mapping extension'lar orqali (AutoMapper o'rniga tez va aniq)

### 2. Soft Delete
- `ISoftDelete` interface bilan
- Global Query Filter avtomatik yashiradi
- `.IgnoreQueryFilters()` bilan trash ko'rish mumkin
- Restore va Permanent Delete admin uchun

### 3. Refresh Token
- Access token 15 daqiqa (qisqa — xavfsiz)
- Refresh token 7 kun (DB'da saqlanadi)
- **Token rotation** — har refresh yangi refresh chiqaradi
- Logout serverda tokenni bekor qiladi (LogoutAll ham bor)

### 4. Concurrency (Order Race Condition)
- `[Timestamp] RowVersion` SaleBook'da
- Transaction ichida stock decrement
- `DbUpdateConcurrencyException` — 3 marta retry (progressive backoff)
- Xato bo'lsa foydalanuvchiga "Kitob boshqa foydalanuvchi tomonidan olindi"

### 5. Rate Limiting
- **Auth policy**: 5 so'rov/daqiqa (brute force himoyasi)
- **General policy**: 100 so'rov/daqiqa
- 429 status code (queue limit 0)

### 6. Structured Logging (Serilog)
- Console + File (kunlik rotation)
- Har HTTP so'rov: metod, path, status, davomiyligi
- Enrichers: CorrelationId, MachineName, ThreadId
- Level filtering: EF Core Warning'dan yuqori

### 7. Health Checks
- `/health` — umumiy status
- `/health/ready` — DB tayyorligi
- `/health/live` — process tirikligi
- Docker/Kubernetes probes uchun

## 🧩 Extension pattern

`Common/Extensions/ClaimsPrincipalExtensions.cs`:
```csharp
User.GetUserId()   // int? qaytaradi (null-safe)
User.IsAdmin()     // Role tekshirish
User.GetEmail()    // Email claim
```

Har controllerda `User.FindFirst(ClaimTypes.NameIdentifier)?.Value` yozish o'rniga.

## 🚀 Kelajakdagi ish

- [ ] Service Layer + Repository Pattern
- [ ] MediatR (CQRS)
- [ ] Docker + docker-compose
- [ ] xUnit testlar (Unit + Integration)
- [ ] Application Insights / Sentry
- [ ] Azure/Render deploy
- [ ] Email service (SMTP)
- [ ] Full-text search (Elasticsearch)
- [ ] Click/Payme integratsiyasi
