# 📋 CHANGELOG

Loyiha versiyasidagi barcha o'zgarishlar bu yerda qayd etiladi.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
Versioning: [SemVer](https://semver.org/lang/uz/)

## [Unreleased]

### 🎨 Clean Code Baseline
- **Common/Constants/** papka — AppConstants, AuthConstants, OrderConstants
- **Common/Extensions/** — ClaimsPrincipalExtensions (GetUserId, IsAdmin, GetEmail)
- Route consistency (`/api/Books` → `/api/books`)
- Magic number'lar konstantalarga chiqarildi
- XML documentation barcha public API'da
- Takrorlangan claim o'qish kodi olib tashlandi
- `docs/ARCHITECTURE.md` va `docs/CONTRIBUTING.md` qo'shildi

## [0.14.0] — 2026-09-09
### ✨ Marketplace HYBRID redesign
- Flash sale banner + real-time countdown
- Filter sidebar (narx, kategoriya, chegirma, wishlist)
- Sort dropdown (yangi, narx, chegirma)
- Wishlist heart (localStorage)
- Discount badge + kesib tashlangan eski narx
- Admin edit modal
- SaleBook.Discount + DiscountEndsAt

## [0.13.0] — 2026-09-09
### ✨ Homepage mega-upgrade
- Hero featured book banner
- Reading stats + streak
- Kun iqtibosi (avto-almashuv)
- Netflix-style "Yaqinda qo'shildi"
- Reading challenges
- Auto-generatsiya SVG kitob muqovalari

## [0.12.0] — 2026-09-09
### ✨ Reader redesign
- StPageFlip integratsiya (fliphtml5 kabi)
- Batch loading (40 sahifa)
- 3500 belgi chunk size

## [0.11.0] — 2026-09-09
### ✨ Backend quality batch
- Custom Exceptions (7 turi)
- FluentValidation
- Response Compression (Brotli + Gzip)
- In-Memory Cache + Output Cache
- CancellationToken

## [0.10.0] — 2026-09-09
### ✨ Observability
- Serilog (console + file, kunlik rotation)
- Health Checks (`/health`, `/health/ready`, `/health/live`)
- Request logging middleware

## [0.9.0] — 2026-09-09
### ✨ Soft Delete
- ISoftDelete interface
- Global Query Filter
- Restore + Permanent delete endpoints

## [0.8.0] — 2026-09-09
### ✨ GitHub Actions CI/CD
- `.github/workflows/build.yml`
- `.github/workflows/pr-checks.yml`
- `.github/dependabot.yml`

## [0.7.0] — 2026-09-09
### ✨ DTO Layer
- Har entity uchun Response/Create/Update DTO
- MappingExtensions
- N+1 query tuzatildi (BookWithStatsDto)

## [0.6.0] — 2026-09-08
### ✨ Enum'lar
- OrderStatus, UserRole
- HasConversion<string>() — DB'da o'qishga oson

## [0.5.0] — 2026-09-07
### 🔐 Refresh Token
- Access 15 daqiqa + Refresh 7 kun
- Token rotation
- Logout va LogoutAll endpoints

## [0.4.0] — 2026-09-07
### 🔒 Order Race Condition
- Transaction + RowVersion (optimistic concurrency)
- 3 marta retry (progressive backoff)

## [0.3.0] — 2026-09-07
### 🔐 Comments Auth
- `[Authorize]` majburiy
- AuthorName JWT'dan avto-olinadi
- 1 kitob = 1 izoh cheklovi

## [0.2.0] — 2026-09-07
### 🔐 JWT User Secrets
- Kalit `dotnet user-secrets` da
- GitHub'dan yashirin

## [0.1.0] — 2026-05-16
### 🎉 Initial release
- CRUD amaliyoti
- JWT autentifikatsiya
- 3D varaqlash
- Marketplace
- Rate Limiting
- Global Exception Handler
