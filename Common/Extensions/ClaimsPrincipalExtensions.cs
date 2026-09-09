using System.Security.Claims;
using KutubxonaAPI.Models.Enums;

namespace KutubxonaAPI.Common.Extensions;

/// <summary>
/// <see cref="ClaimsPrincipal"/> uchun yordamchi extension metodlari.
/// Har controllerda takrorlanadigan claim o'qish kodini olib tashlaydi.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// JWT tokendan foydalanuvchi ID sini oladi.
    /// </summary>
    /// <returns>Muvaffaqiyatli bo'lsa ID, aks holda null.</returns>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var idStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : null;
    }

    /// <summary>
    /// Foydalanuvchi rolini oladi.
    /// </summary>
    public static UserRole? GetUserRole(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(role, out var r) ? r : null;
    }

    /// <summary>
    /// Foydalanuvchi Admin ekanligini tekshiradi.
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole(nameof(UserRole.Admin));

    /// <summary>
    /// Foydalanuvchi email manzilini oladi.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Email)?.Value;
}
