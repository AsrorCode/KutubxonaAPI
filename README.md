# 📚 KutubxonaAPI

**O'zbek onlayn kutubxona** — kitob o'qish, sotib olish va boshqarish uchun to'liq platforma.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp)
![JWT](https://img.shields.io/badge/JWT-000?logo=jsonwebtokens)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver)
[![CI](https://github.com/AsrorCode/KutubxonaAPI/actions/workflows/build.yml/badge.svg)](https://github.com/AsrorCode/KutubxonaAPI/actions)

---

## ✨ Nima bor?

**Foydalanuvchi**: 3D varaqlash bilan kitob o'qish · Izoh va yulduzli reyting · O'qish davom etishi · Marketplace'dan xarid · Buyurtmalar tarixi · Reading streak · Wishlist · Kunlik iqtiboslar

**Admin**: Kitob boshqaruvi · PDF yuklab avto-sahifalash · Marketplace CRUD · Chegirmalar boshqaruvi · Buyurtma statusini o'zgartirish · Tahlil va statistika

**Xavfsizlik**: JWT + Refresh Token (15 daq access, 7 kun refresh) · BCrypt hash · Rate limiting · Kuchli parol · CORS · Global exception handler · Soft Delete

---

## 🛠️ Stack

**Backend**: .NET 10, ASP.NET Core, EF Core 10, SQL Server, JWT, BCrypt, Serilog, FluentValidation, Scalar  
**Frontend**: Vanilla HTML/CSS/JS, Dark Premium Design, StPageFlip, PDF.js  
**DevOps**: GitHub Actions CI/CD, Dependabot

---

## 🚀 Ishga tushirish

```bash
git clone https://github.com/AsrorCode/KutubxonaAPI.git
cd KutubxonaAPI
dotnet user-secrets set "Jwt:Key" "SIZNING_MAXFIY_KALITINGIZ"
dotnet ef database update
dotnet run
```

Ochish: `http://localhost:5000` · API docs: `/scalar/v1`

---

## 📡 Asosiy endpointlar

| Metod | Endpoint | Auth |
|-------|----------|------|
| POST | `/api/auth/register`, `/login`, `/refresh`, `/logout` | ❌ / ✅ |
| GET | `/api/books?page=1&pageSize=20` | ❌ |
| POST | `/api/books` | 👮 Admin |
| GET | `/api/books/{id}/pages/{n}` | ❌ |
| POST | `/api/books/{id}/comments` | ✅ |
| GET | `/api/salebooks` | ❌ |
| POST | `/api/orders` | ✅ |
| GET | `/health`, `/health/ready`, `/health/live` | ❌ |

To'liq ro'yxat: `/scalar/v1` (loyiha ishga tushgach)

---

## 📁 Loyiha strukturasi

```
Controllers/  Common/  Data/  DTOs/  Exceptions/
Middleware/   Migrations/  Models/  Validators/
wwwroot/  docs/  .github/
```

Batafsil: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

---

## 📚 Dokumentatsiya

- 🏛️ [ARCHITECTURE](docs/ARCHITECTURE.md) — Arxitektura qarorlari
- 🤝 [CONTRIBUTING](docs/CONTRIBUTING.md) — Hissa qo'shish qoidalari
- 📋 [CHANGELOG](CHANGELOG.md) — Versiya tarixi

---

## 🔮 Kelajakda

Service Layer + Repository · MediatR (CQRS) · Docker · xUnit testlar · Email service · Click/Payme · Blazor WASM · Full-text search

---

## 👨‍💻 Muallif

**Asror Haydarov** · 📧 asroh131@gmail.com · 🐙 [@AsrorCode](https://github.com/AsrorCode)

MIT License · Made in Uzbekistan 🇺🇿
