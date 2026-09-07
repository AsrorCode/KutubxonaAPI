# 📚 KutubxonaAPI

**O'zbek onlayn kutubxona** — kitob o'qish, sotib olish va boshqarish uchun to'liq platforma.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) ![C#](https://img.shields.io/badge/C%23-239120?logo=csharp) ![JWT](https://img.shields.io/badge/JWT-000?logo=jsonwebtokens) ![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver)

---

## ✨ Nima bor?

**Foydalanuvchi uchun**: 3D varaqlash bilan kitob o'qish · Izoh va yulduzli reyting · O'qish davom etishi · Marketplace'dan xarid · Buyurtmalar tarixi

**Admin uchun**: Kitob boshqaruvi · PDF yuklab avto-sahifalash · Marketplace CRUD · Buyurtma statusini o'zgartirish · Statistika

**Xavfsizlik**: JWT + Refresh Token (15 daq access, 7 kun refresh) · BCrypt hash · Rate limiting · Kuchli parol · CORS · Global exception handler

---

## 🛠️ Stack

**Backend**: .NET 10, ASP.NET Core Web API, EF Core 10, SQL Server LocalDB, JWT, BCrypt, Scalar  
**Frontend**: Vanilla HTML/CSS/JS, Dark Premium (Glassmorphism), PDF.js  

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

To'liq ro'yxat: `/scalar/v1` (loyiha ishga tushgach)

---

## 📁 Struktura

```
Controllers/  · Models/  · Data/AppDbContext.cs  · DTOs/
Middleware/   · Migrations/  · wwwroot/  · Program.cs
```

---

## 🔮 Kelajakda

Sevimli kitoblar · Foydalanuvchi profili · Email tasdiqlash · Parol tiklash · Click/Payme · Telegram bot · Docker

---

## 👨‍💻 Muallif

**Asror Haydarov** · 📧 asroh131@gmail.com · 🐙 [@AsrorCode](https://github.com/AsrorCode)

MIT License · Made in Uzbekistan 🇺🇿
