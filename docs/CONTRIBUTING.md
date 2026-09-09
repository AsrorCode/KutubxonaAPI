# 🤝 Hissa qo'shish qoidalari

Loyihaga hissa qo'shishga qiziqganingiz uchun rahmat! Bu hujjat sizga qanday
qilib to'g'ri ishlash haqida ma'lumot beradi.

## 🌿 Branch nomlash

Har turdagi ish uchun **prefix**:

| Prefix | Vazifasi | Misol |
|--------|---------|-------|
| `feature/` | Yangi funksiya | `feature/favorites` |
| `fix/` | Xato tuzatish | `fix/order-race-condition` |
| `refactor/` | Kod qayta yozish | `refactor/service-layer` |
| `test/` | Testlar | `test/book-controller` |
| `docs/` | Dokumentatsiya | `docs/api-guide` |
| `devops/` | Deploy/CI | `devops/docker` |

## 📝 Commit xabarlari

Emoji + qisqa tavsif:

```
🎨 style: format code
🔧 refactor: extract magic numbers
✨ feat: add favorites
🐛 fix: order race condition
📝 docs: update README
✅ test: add BookService tests
🚀 perf: cache category list
🔒 security: add CSRF protection
```

## 🎯 Clean Code qoidalari

### Nomlash
- **Klasslar**: `PascalCase` — `BookService`, `OrderRepository`
- **Metodlar**: `PascalCase` — `GetBookById`, `CreateOrder`
- **Xususiyatlar**: `PascalCase` — `Title`, `CreatedAt`
- **Lokal o'zgaruvchilar**: `camelCase` — `userId`, `bookList`
- **Konstantalar**: `PascalCase` — `MaxRetries`, `DefaultPageSize`
- **Interfaceler**: `IPascalCase` — `IBookRepository`

### DTO'lar
- Har entity uchun **3 xil DTO**:
  - `EntityResponseDto` — API'dan qaytadi
  - `EntityCreateDto` — POST uchun
  - `EntityUpdateDto` — PUT uchun
- **Hech qachon** entity to'g'ridan qaytarilmaydi

### Async patterns
- Barcha DB operatsiyalari **async**
- `CancellationToken` — hamma joyda
- `async void` **hech qachon** (event handler'lardan tashqari)
- `Task.WhenAll()` — parallel operatsiyalar

### Exception handling
- **Custom exceptions** (NotFound, Conflict, BusinessException...)
- Global exception middleware ushlaydi
- `throw new Exception(...)` — **hech qachon** generic

### Constants
- **Magic numbers taqiqlangan**
- `Common/Constants/` — hamma joyda
- Ma'nosi bor nomlar bilan

## 🔄 Ish oqimi

1. Yangi branch yarating (yuqoridagi prefix bilan)
2. Kod yozing (kichik commitlar)
3. Testlarni ishga tushiring: `dotnet test`
4. Push qiling: `git push -u origin feature/xxx`
5. GitHub'da PR oching
6. CI pipeline yashil bo'lguncha kuting
7. Reviewer'lar tasdiqlagach — merge

## 🧪 Testing

```bash
# Barcha testlar
dotnet test

# Bitta project
dotnet test tests/KutubxonaAPI.UnitTests

# Coverage bilan
dotnet test /p:CollectCoverage=true
```

## 📚 Foydali havolalar

- [Microsoft Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)
- [EF Core Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)
