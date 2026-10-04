# KutubxonaAPI 📚

O'zbek tilidagi onlayn kitob do'koni (marketplace) — ASP.NET Core backend va vanilla HTML/CSS/JS frontend. Portfolio + real biznes loyihasi.

## Texnologiyalar

| Qatlam | Texnologiya |
|--------|-------------|
| Framework | .NET 10, ASP.NET Core Web API |
| ORM | Entity Framework Core 10 |
| Ma'lumotlar bazasi | SQL Server (LocalDB) |
| Autentifikatsiya | JWT Bearer + Refresh Token (rotation), BCrypt |
| Validatsiya | FluentValidation |
| Loglar | Serilog (Console + File) |
| Hujjat | OpenAPI + Scalar |
| Frontend | Vanilla HTML / CSS / JS (`wwwroot`) |
| Testlar | xUnit |

## Asosiy imkoniyatlar

- **Marketplace** — kitob katalogi, qidiruv + autocomplete, kategoriya filtri, chegirmalar, rasm galereyasi
- **Buyurtmalar** — savat, transaction + concurrency himoyali buyurtma yaratish, status kuzatuvi, stock boshqaruvi
- **Autentifikatsiya** — ro'yxatdan o'tish, kirish, refresh token, parol tiklash, email tasdiqlash
- **Ijtimoiy** — sharhlar va reyting, "Ovoz bering" (o'qish holati), sevimlilar (wishlist), bildirishnomalar
- **Shaxsiy** — "Siz uchun" tavsiya, profil + avatar, kolleksiyalar
- **Admin** — kitob CRUD, buyurtma boshqaruvi, statistika dashboard (grafiklar)

## Xavfsizlik va ishlash

- JWT imzo kaliti va DB paroli **`dotnet user-secrets`da** (hech qachon repo'ga tushmaydi)
- Rate limiting — auth endpoint'lar (5/min) + global limiter (100/min har IP)
- Soft delete + global query filter, optimistik concurrency (RowVersion)
- Response compression (Brotli/Gzip), Output cache (kitob ro'yxatlari 30s)
- Sahifalash (opt-in `?page&pageSize`), fon tozalash xizmati (eskirgan token/bildirishnoma)
- Global exception middleware, health checks (`/health`, `/health/ready`, `/health/live`)

## Ishga tushirish

### Talablar
- .NET 10 SDK
- SQL Server LocalDB (yoki boshqa SQL Server)

### Qadamlar

```bash
# 1. Repo
git clone https://github.com/AsrorCode/KutubxonaAPI.git
cd KutubxonaAPI

# 2. Sirlarni o'rnatish (MAJBURIY)
dotnet user-secrets set "Jwt:Key" "<kamida-32-belgili-maxfiy-kalit>"

# 3. Bazani yaratish
dotnet ef database update

# 4. Ishga tushirish
dotnet run
```

Sayt: `https://localhost:5001` — API hujjati (dev): `https://localhost:5001/scalar/v1`

### Testlar

```bash
dotnet test Tests/KutubxonaAPI.Tests.csproj
```

## Loyiha tuzilmasi

```
Controllers/     — API endpointlar (SaleBooks, Orders, Auth, Reviews, Votes, ...)
Models/          — EF entity'lar
DTOs/            — so'rov/javob obyektlari + mapping
Validators/      — FluentValidation qoidalari
Services/        — Email, fon tozalash xizmati
Data/            — AppDbContext
Common/          — konstantalar, extension'lar, pagination
Middleware/      — global exception handler
Migrations/      — EF migratsiyalar
Tests/           — xUnit testlar (alohida loyiha)
wwwroot/         — frontend (market, admin, profil, auth sahifalari)
```

## Muhit sozlamalari

| Kalit | Tavsif |
|-------|--------|
| `Jwt:Key` | JWT imzo kaliti (user-secrets) |
| `ConnectionStrings:DefaultConnection` | Baza ulanishi |
| `Email:Smtp:*` | SMTP (ixtiyoriy — bo'lmasa havola logga yoziladi) |
| `AutoMigrate` | Production'da avtomatik migratsiya (default: false) |
| `AllowedOrigins` | CORS uchun ruxsat etilgan manzillar |

## Litsenziya

Shaxsiy / ta'lim loyihasi.
