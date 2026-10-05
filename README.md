# Zarvaraq 📖

> O'zbek tilidagi onlayn kitob do'koni (marketplace) — ASP.NET Core backend va vanilla HTML/CSS/JS frontend.
> Portfolio + real biznes loyihasi.

<p>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/PostgreSQL-316192?style=flat-square&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/JWT-auth-000000?style=flat-square&logo=jsonwebtokens&logoColor=white" />
  <img src="https://img.shields.io/badge/tests-xUnit-5CB85C?style=flat-square" />
  <img src="https://img.shields.io/badge/CI-GitHub%20Actions-2088FF?style=flat-square&logo=githubactions&logoColor=white" />
</p>

---

## Texnologiyalar

| Qatlam | Texnologiya |
|--------|-------------|
| Framework | .NET 10, ASP.NET Core Web API |
| ORM | Entity Framework Core 10 |
| Ma'lumotlar bazasi | PostgreSQL (Npgsql) |
| Autentifikatsiya | JWT Bearer + Refresh Token (rotation), BCrypt |
| Validatsiya | FluentValidation |
| Loglar | Serilog (Console + File) |
| API hujjati | OpenAPI + Scalar |
| Frontend | Vanilla HTML / CSS / JS (`wwwroot`) |
| Testlar | xUnit |
| CI/CD | GitHub Actions (build + test) |

---

## Asosiy imkoniyatlar

- **Marketplace** — kitob katalogi, jonli qidiruv (autocomplete), kategoriya filtri, chegirmalar, **rasm galereyasi** (bir kitobga bir nechta rasm), hero bo'limi
- **Buyurtmalar** — savat, transaction + concurrency himoyali buyurtma yaratish, status kuzatuvi (timeline), stock boshqaruvi
- **Autentifikatsiya** — ro'yxatdan o'tish, kirish, refresh token rotation, parol tiklash, email tasdiqlash
- **Ijtimoiy** — sharhlar va reyting, **"Ovoz bering"** (o'qish holati), sevimlilar (wishlist), sayt ichi bildirishnomalar
- **Shaxsiy** — "Siz uchun" tavsiya feed, profil + avatar, kolleksiyalar (to'plamlar)
- **Admin** — kitob CRUD (bir nechta rasm bilan), buyurtma boshqaruvi, statistika dashboard (grafiklar)
- **Dizayn** — iliq "editorial + boutique" uslub, Lotincha/Kirilcha tanlagich, yorug'/qorong'i mavzu

---

## Xavfsizlik va ishlash

- JWT imzo kaliti va DB paroli **`dotnet user-secrets`da** (hech qachon repo'ga tushmaydi)
- Rate limiting — auth endpoint'lar (5/min) + global limiter (100/min har IP)
- Soft delete + global query filter, optimistik concurrency (PostgreSQL `xmin`)
- Response compression (Brotli/Gzip), Output cache (kitob ro'yxatlari 30s)
- Sahifalash (opt-in `?page&pageSize`), fon tozalash xizmati (eskirgan token/bildirishnoma)
- Global exception middleware, health checks (`/health`, `/health/ready`, `/health/live`), HSTS

---

## Ishga tushirish

### Talablar
- .NET 10 SDK
- PostgreSQL (lokal yoki Docker)

### Qadamlar

```bash
# 1. Repo
git clone https://github.com/AsrorCode/KutubxonaAPI.git
cd KutubxonaAPI

# 2. Sirlarni o'rnatish (MAJBURIY)
dotnet user-secrets set "Jwt:Key" "<kamida-32-belgili-maxfiy-kalit>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=zarvaraq;Username=postgres;Password=<parol>"

# 3. Bazani yaratish (PostgreSQL)
dotnet ef database update

# 4. Ishga tushirish
dotnet run
```

Sayt: `https://localhost:5001` — API hujjati (dev): `https://localhost:5001/scalar/v1`

### Testlar

```bash
dotnet test Tests/KutubxonaAPI.Tests.csproj
```

---

## Loyiha tuzilmasi

```
Controllers/   — API endpointlar (SaleBooks, Orders, Auth, Reviews, Votes, ...)
Models/        — EF entity'lar
DTOs/          — so'rov/javob obyektlari + mapping
Validators/    — FluentValidation qoidalari
Services/      — Email, fon tozalash xizmati
Data/          — AppDbContext + seeder
Common/        — konstantalar, extension'lar, pagination
Middleware/    — global exception handler
Migrations/    — EF migratsiyalar
Tests/         — xUnit testlar (alohida loyiha)
wwwroot/       — frontend (market, admin, profil, auth sahifalari)
```

---

## Muhit sozlamalari

| Kalit | Tavsif |
|-------|--------|
| `Jwt:Key` | JWT imzo kaliti (user-secrets) |
| `ConnectionStrings:DefaultConnection` | PostgreSQL ulanishi |
| `Email:Smtp:*` | SMTP (ixtiyoriy — bo'lmasa tiklash havolasi logga yoziladi) |
| `AutoMigrate` | Production'da avtomatik migratsiya (default: `false`) |
| `AllowedOrigins` | CORS uchun ruxsat etilgan manzillar |

---

## Litsenziya

Shaxsiy / ta'lim loyihasi.
