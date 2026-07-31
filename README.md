# 📚 KutubxonaAPI — O'zbek Onlayn Kutubxona

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-blue?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens&logoColor=white)

**To'liq funksional o'zbek onlayn kutubxona platformasi**  
Kitob o'qish · Sotib olish · Izohlar · 3D varaqlash · Admin panel

[Xususiyatlar](#-xususiyatlar) · [Texnologiyalar](#-texnologiyalar) · [O'rnatish](#-ornatish) · [API](#-api-endpointlar) · [Ekran suratlar](#-ekran-suratlar)

</div>

---

## 📖 Loyiha haqida

**KutubxonaAPI** — bu o'zbek adabiyotini onlayn o'qish va sotib olish uchun mo'ljallangan zamonaviy web platforma. Foydalanuvchilar bepul kitoblarni **3D varaqlash effekti** bilan o'qishlari, sevimlilariga izoh va reyting qoldirishlari, hamda kitob **sotib olish** imkoniyatlariga ega.

Loyiha **.NET 10** va **ASP.NET Core** asosida qurilgan, JWT autentifikatsiya va role-based avtorizatsiya bilan ta'minlangan.

---

## ✨ Xususiyatlar

### 👥 Foydalanuvchilar uchun
- 📚 **Bepul o'qish** — Kitoblarni onlayn o'qish (3D varaqlash effekti)
- 🔍 **Qidiruv va filtrlash** — Nom, muallif, kategoriya bo'yicha
- 💬 **Izoh va reyting** — 1-5 yulduzli baholash tizimi
- ❤️ **O'qish davom etishi** — Har kitob uchun progress avtomatik saqlanadi
- 🛒 **Marketplace** — Kitoblarni sotib olish
- 📦 **Buyurtmalar tarixi** — O'z buyurtmalarni kuzatish
- 🌅 **Salomlashuv** — Vaqtga qarab shaxsiy salomlashuv (tong/kun/kech/tun)

### 🛡️ Admin uchun
- ⚙️ **Kitoblarni boshqarish** — Qo'shish, tahrirlash, o'chirish
- 📄 **PDF yuklash** — PDF'dan matn avtomatik ajratiladi va sahifalarga bo'linadi (PDF.js)
- 💼 **Sotuv boshqaruvi** — SaleBooks bilan ishlash
- 📊 **Statistika** — Kitoblar, kategoriyalar, buyurtmalar
- 🗑️ **Izoh moderatsiya** — Nomaqbul izohlarni o'chirish

### 🔒 Xavfsizlik
- 🔐 **JWT Bearer autentifikatsiya** — 7 kun amal qiladigan tokenlar
- 🔑 **BCrypt parol hash** — Salt bilan xavfsiz saqlash
- 👮 **Role-based avtorizatsiya** — User / Admin rollari
- 🚫 **Rate Limiting** — Brute force hujumlaridan himoya (5 so'rov/daqiqa)
- ✅ **Kuchli parol validatsiya** — Minimum 8 belgi, katta+kichik harf+raqam
- 🛡️ **Global Exception Handler** — Xatolar xavfsiz ushlanadi
- 🌐 **CORS boshqaruvi** — Faqat ruxsat berilgan domenlar

---

## 🛠️ Texnologiyalar

### Backend
| Texnologiya | Versiya | Vazifasi |
|-------------|---------|----------|
| **.NET** | 10.0 | Framework |
| **ASP.NET Core Web API** | 10.0 | REST API |
| **Entity Framework Core** | 10.0 | ORM |
| **SQL Server LocalDB** | — | Ma'lumotlar bazasi |
| **JWT Bearer** | 10.0 | Autentifikatsiya |
| **BCrypt.Net-Next** | 4.2 | Parol hashing |
| **Scalar** | Latest | API dokumentatsiya |
| **Serilog** | Latest | Logging |

### Frontend
| Texnologiya | Vazifasi |
|-------------|----------|
| **Vanilla HTML/CSS/JS** | UI |
| **Inter Font** | Typography |
| **Glassmorphism Design** | Zamonaviy dizayn |
| **PDF.js** | PDF matn ajratish |
| **LocalStorage** | Progress saqlash, auth token |

### Dizayn
- 🎨 **Dark Premium tema** — Glassmorphism, purple/cyan gradient
- 📱 **Responsive** — Mobile-friendly
- ✨ **Animatsiyalar** — Silliq o'tishlar, 3D varaqlash

---

## 🚀 O'rnatish

### Talablar
- **.NET 10 SDK** — [Yuklab olish](https://dotnet.microsoft.com/download)
- **SQL Server LocalDB** (Visual Studio bilan keladi)
- **Visual Studio 2022** yoki **VS Code**

### Qadamma-qadam

**1. Loyihani klon qiling:**
```bash
git clone https://github.com/AsrorCode/KutubxonaAPI.git
cd KutubxonaAPI
```

**2. Paketlarni tiklang:**
```bash
dotnet restore
```

**3. Ma'lumotlar bazasini yarating:**
```bash
dotnet ef database update
```

**4. Loyihani ishga tushiring:**
```bash
dotnet run
```

**5. Brauzer'da oching:**
- Frontend: `https://localhost:5001`
- API docs: `https://localhost:5001/scalar/v1`

---

## ⚙️ Konfiguratsiya

`appsettings.json` faylini o'zgartiring:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=KutubxonaDB;Trusted_Connection=True"
  },
  "Jwt": {
    "Key": "SIZNING_MAXFIY_KALITINGIZ_KAMIDA_32_BELGI",
    "Issuer": "KutubxonaAPI",
    "Audience": "KutubxonaUsers",
    "ExpireDays": 7
  },
  "AllowedOrigins": [
    "http://localhost:5000",
    "https://localhost:5001"
  ]
}
```

---

## 📡 API Endpointlar

### 🔐 Auth
| Method | Endpoint | Auth | Tavsif |
|--------|----------|------|--------|
| POST | `/api/auth/register` | ❌ | Ro'yxatdan o'tish |
| POST | `/api/auth/login` | ❌ | Kirish |
| GET | `/api/auth/me` | ✅ | Joriy foydalanuvchi |

### 📚 Books
| Method | Endpoint | Auth | Tavsif |
|--------|----------|------|--------|
| GET | `/api/books?page=1&pageSize=20` | ❌ | Pagination bilan ro'yxat |
| GET | `/api/books/{id}` | ❌ | Bitta kitob |
| GET | `/api/books/categories` | ❌ | Kategoriyalar |
| POST | `/api/books` | 👮 Admin | Yaratish |
| PUT | `/api/books/{id}` | 👮 Admin | Yangilash |
| DELETE | `/api/books/{id}` | 👮 Admin | O'chirish |

### 📄 Book Pages
| Method | Endpoint | Auth | Tavsif |
|--------|----------|------|--------|
| GET | `/api/books/{id}/pages` | ❌ | Sahifalar ro'yxati |
| GET | `/api/books/{id}/pages/{n}` | ❌ | Bitta sahifa |
| POST | `/api/books/{id}/pages/bulk` | 👮 Admin | PDF matnni yuklash |
| DELETE | `/api/books/{id}/pages` | 👮 Admin | Barcha sahifalarni o'chirish |

### 💬 Comments
| Method | Endpoint | Auth | Tavsif |
|--------|----------|------|--------|
| GET | `/api/books/{id}/comments` | ❌ | Izohlar + o'rtacha reyting |
| POST | `/api/books/{id}/comments` | ❌ | Izoh qoldirish |
| DELETE | `/api/books/{id}/comments/{cid}` | 👮 Admin | O'chirish |

### 🛒 SaleBooks (Marketplace)
| Method | Endpoint | Auth | Tavsif |
|--------|----------|------|--------|
| GET | `/api/salebooks` | ❌ | Sotuvdagi kitoblar |
| POST | `/api/salebooks` | 👮 Admin | Yaratish |
| PATCH | `/api/salebooks/{id}/toggle` | 👮 Admin | Faol/nofaol |

### 📦 Orders
| Method | Endpoint | Auth | Tavsif |
|--------|----------|------|--------|
| POST | `/api/orders` | ✅ | Buyurtma yaratish |
| GET | `/api/orders/my` | ✅ | Mening buyurtmalarim |
| GET | `/api/orders` | 👮 Admin | Barcha buyurtmalar |
| PATCH | `/api/orders/{id}/status` | 👮 Admin | Statusni o'zgartirish |

---

## 📁 Loyiha strukturasi

KutubxonaAPI/
├── Controllers/ # API kontrollerlar
│ ├── AuthController.cs
│ ├── BooksController.cs
│ ├── BookPagesController.cs
│ ├── CommentsController.cs
│ ├── SaleBooksController.cs
│ └── OrdersController.cs
├── Models/ # Entity klasslar
│ ├── Book.cs
│ ├── BookPage.cs
│ ├── Comment.cs
│ ├── User.cs
│ ├── SaleBook.cs
│ ├── Order.cs
│ └── OrderItem.cs
├── Data/
│ └── AppDbContext.cs # EF Core DbContext
├── DTOs/ # Data Transfer Objects
│ └── PagedResult.cs
├── Middleware/
│ └── GlobalExceptionMiddleware.cs
├── Migrations/ # EF migratsiyalar
├── wwwroot/ # Frontend fayllar
│ ├── index.html # Bosh sahifa (kutubxona)
│ ├── market.html # Marketplace
│ ├── admin.html # Admin panel
│ ├── login.html # Kirish
│ ├── register.html # Ro'yxatdan o'tish
│ └── my-orders.html # Buyurtmalarim
├── appsettings.json
├── Program.cs
└── KutubxonaAPI.csproj


---

## 📸 Ekran suratlar

> Ekran suratlar tez orada qo'shiladi

---

## 🔮 Rejalar (Roadmap)

### ✅ Bajarilgan
- [x] CRUD amallar
- [x] JWT autentifikatsiya
- [x] Role-based avtorizatsiya
- [x] 3D varaqlash effekti
- [x] PDF matn ajratish
- [x] Dark Premium dizayn
- [x] Marketplace
- [x] Pagination
- [x] Rate Limiting
- [x] Global Exception Handler

### 🚧 Rejada
- [ ] ❤️ Sevimli kitoblar
- [ ] 👤 Foydalanuvchi profili sahifasi
- [ ] 📧 Email tasdiqlash
- [ ] 🔑 Parolni tiklash
- [ ] 🔄 Refresh token
- [ ] 💳 Click/Payme integratsiyasi
- [ ] 📱 Telegram bot
- [ ] 🎧 Audio kitoblar
- [ ] 🔔 SignalR real-time bildirishnomalar
- [ ] 🐳 Docker containerization
- [ ] ☁️ Azure/Render deploy
- [ ] 🧪 xUnit testlar

---

## 🤝 Ishtirok etish

Loyihaga hissa qo'shishni istaysizmi? Ajoyib!

1. **Fork** qiling
2. Yangi branch yarating (`git checkout -b feature/AjoyibXususiyat`)
3. O'zgarishlarni commit qiling (`git commit -m 'Ajoyib xususiyat qo'shildi'`)
4. Branch'ga push qiling (`git push origin feature/AjoyibXususiyat`)
5. **Pull Request** oching

---

## 📝 Litsenziya

Bu loyiha **MIT** litsenziyasi ostida tarqatiladi. Batafsil ma'lumot uchun [LICENSE](LICENSE) fayliga qarang.

---

## 👨‍💻 Muallif

**Asror Haydarov**  
📧 asroh131@gmail.com  
🐙 GitHub: [@AsrorCode](https://github.com/AsrorCode)

---

## 🙏 Minnatdorchilik

- **Anthropic Claude** — Loyihani qurishda yordam bergani uchun
- **Microsoft** — .NET va ASP.NET Core uchun
- **Scalar** — Chiroyli API dokumentatsiya uchun
- **O'zbek adabiyoti mualliflari** — Bizga ilhom bergani uchun

---

<div align="center">

**⭐ Agar loyiha yoqqan bo'lsa, GitHub'da yulduzcha qo'ying!**

Made with ❤️ in Uzbekistan 🇺🇿

</div>
