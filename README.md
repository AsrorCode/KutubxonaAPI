# Kutubxona API — .NET CRUD Loyihasi

Bu loyiha **ASP.NET Core Web API** asosida yaratilgan oddiy CRUD ilovasidir. Kitoblar kutubxonasini boshqarish uchun mo'ljallangan.

## Texnologiyalar

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server (LocalDB)
- Scalar (zamonaviy API hujjatlari UI)
- OpenAPI / Swagger spec

## Loyiha tuzilishi

```
KutubxonaAPI/
├── Controllers/
│   └── BooksController.cs      ← API endpointlar (GET, POST, PUT, DELETE)
├── Models/
│   └── Book.cs                 ← Kitob modeli (ma'lumotlar bazasi entity'si)
├── Data/
│   └── AppDbContext.cs         ← EF Core DbContext
├── Properties/
│   └── launchSettings.json     ← Ishga tushirish sozlamalari
├── appsettings.json            ← Konfiguratsiya (connection string)
├── Program.cs                  ← Asosiy entry point
└── KutubxonaAPI.csproj         ← Loyiha fayli (NuGet paketlari)
```

## Talablar

Loyihani ishga tushirish uchun quyidagilar o'rnatilgan bo'lishi kerak:

1. **.NET 8 SDK** — https://dotnet.microsoft.com/download
2. **SQL Server LocalDB** — Visual Studio bilan birga keladi, yoki alohida o'rnatish mumkin
3. **Visual Studio 2022** yoki **VS Code** (ixtiyoriy, lekin tavsiya etiladi)

## Loyihani ishga tushirish

### 1. Loyiha papkasiga kirish

```bash
cd KutubxonaAPI
```

### 2. NuGet paketlarini tiklash

```bash
dotnet restore
```

### 3. Loyihani build qilish

```bash
dotnet build
```

### 4. Ishga tushirish

```bash
dotnet run
```

Ilova ishga tushgandan so'ng brauzeringizda quyidagi manzilni oching:

```
https://localhost:5001/scalar/v1
```

Bu yerda **Scalar UI** orqali barcha endpointlarni ko'rishingiz va sinab ko'rishingiz mumkin. Scalar — Swagger'dan ko'ra zamonaviy va qulay interfeys.

## Migratsiya (ixtiyoriy, lekin professional usul)

Loyiha hozir `EnsureCreated()` orqali DB'ni avtomatik yaratadi. Lekin haqiqiy loyihalarda **EF Core Migrations** ishlatish kerak.

Migratsiyani sozlash:

```bash
# EF tools'ni o'rnatish (faqat bir marta)
dotnet tool install --global dotnet-ef

# Birinchi migratsiya yaratish
dotnet ef migrations add InitialCreate

# Migratsiyani ma'lumotlar bazasiga qo'llash
dotnet ef database update
```

> Eslatma: Migratsiyadan foydalansangiz, `Program.cs`'dagi `EnsureCreated()` o'rniga `Migrate()` ishlatishingiz kerak.

## API Endpointlar

| Metod  | URL                                | Tavsifi                            |
|--------|------------------------------------|------------------------------------|
| GET    | `/api/books`                       | Barcha kitoblarni olish            |
| GET    | `/api/books/{id}`                  | Bitta kitobni ID bo'yicha olish    |
| GET    | `/api/books/search?query=...`      | Qidiruv (nomi yoki muallif bo'yicha) |
| POST   | `/api/books`                       | Yangi kitob qo'shish               |
| PUT    | `/api/books/{id}`                  | Kitobni to'liq yangilash           |
| PATCH  | `/api/books/{id}/status`           | Faqat holatni o'zgartirish         |
| DELETE | `/api/books/{id}`                  | Kitobni o'chirish                  |

### Misol: Yangi kitob qo'shish (POST)

**URL:** `POST /api/books`

**Body (JSON):**
```json
{
  "title": "Sariq devni minib",
  "author": "Xudoyberdi To'xtaboyev",
  "year": 1971,
  "category": "Bolalar",
  "isAvailable": true
}
```

**Javob (201 Created):**
```json
{
  "id": 4,
  "title": "Sariq devni minib",
  "author": "Xudoyberdi To'xtaboyev",
  "year": 1971,
  "category": "Bolalar",
  "isAvailable": true,
  "createdAt": "2026-05-09T12:34:56Z",
  "updatedAt": null
}
```

### Misol: Kitobni yangilash (PUT)

**URL:** `PUT /api/books/4`

**Body (JSON):**
```json
{
  "title": "Sariq devni minib",
  "author": "Xudoyberdi To'xtaboyev",
  "year": 1971,
  "category": "Bolalar",
  "isAvailable": false
}
```

## CRUD nima?

CRUD — bu har qanday ma'lumot bilan ishlovchi ilovaning 4 ta asosiy amalini bildiradi:

| Harf | Ma'nosi | HTTP metod | Misol |
|------|---------|------------|-------|
| **C** | Create (Yaratish) | POST | Yangi kitob qo'shish |
| **R** | Read (O'qish) | GET | Kitoblarni ko'rish |
| **U** | Update (Yangilash) | PUT/PATCH | Kitob ma'lumotini o'zgartirish |
| **D** | Delete (O'chirish) | DELETE | Kitobni o'chirish |

## Test qilish (Postman / Swagger orqali)

### Scalar orqali:
1. `dotnet run` ishga tushiring
2. `https://localhost:5001/scalar/v1` ochiling
3. Chap tomondagi endpoint ro'yxatidan birini tanlang
4. O'ng panelda "Test Request" qismida ma'lumot kiriting
5. "Send" tugmasini bosing va javobni ko'ring

### Postman orqali:
1. Postman'ni oching
2. Yangi request yarating
3. URL'ni kiriting (masalan: `https://localhost:5001/api/books`)
4. Metodni tanlang (GET, POST, va h.k.)
5. POST/PUT uchun Body → raw → JSON tanlang va ma'lumotni kiriting
6. "Send" bosing

## Keyingi qadamlar — loyihani rivojlantirish

Bu loyihani yanada yaxshilash uchun quyidagilarni qo'shing:

1. **Authentication / Authorization** — JWT token bilan
2. **DTO (Data Transfer Objects)** — API uchun alohida modellar
3. **Repository Pattern** — Database mantig'ini ajratish
4. **Service Layer** — Biznes mantiq uchun alohida qatlam
5. **AutoMapper** — Modellar orasida avtomatik konversiya
6. **FluentValidation** — Murakkab validatsiya qoidalari
7. **Pagination** — Ko'p ma'lumotlarni sahifalab ko'rsatish
8. **Logging (Serilog)** — Professional loglar
9. **Unit Tests (xUnit)** — Testlar yozish
10. **Frontend** — React, Angular yoki Blazor bilan UI

## Foydali havolalar

- [.NET hujjatlari](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Swagger](https://swagger.io/)

## Muammolar

### "Cannot connect to database" xatosi
- SQL Server LocalDB o'rnatilganini tekshiring: `sqllocaldb info`
- Agar yo'q bo'lsa: `sqllocaldb create MSSQLLocalDB` buyrug'ini bajaring

### Port band xatosi
- `Properties/launchSettings.json` faylida portni o'zgartiring

### "dotnet" buyrug'i topilmadi
- .NET SDK o'rnatilganligini tekshiring: `dotnet --version`

---

Muvaffaqiyat tilayman! Savollar bo'lsa, so'rang.
