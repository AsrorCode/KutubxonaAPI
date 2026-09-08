namespace KutubxonaAPI.Exceptions;

/// <summary>
/// Barcha ilova exception'lari uchun asosiy klass.
/// Har biri o'z HTTP status code'iga ega.
/// </summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    public string ErrorCode { get; }

    protected AppException(string message, string errorCode = "APP_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected AppException(string message, Exception innerException, string errorCode = "APP_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>404 — Ma'lumot topilmadi.</summary>
public class NotFoundException : AppException
{
    public override int StatusCode => 404;

    public NotFoundException(string message)
        : base(message, "NOT_FOUND") { }

    public NotFoundException(string entity, object id)
        : base($"{entity} (ID: {id}) topilmadi", "NOT_FOUND") { }
}

/// <summary>400 — Yomon so'rov (validatsiya, biznes qoidasi).</summary>
public class BadRequestException : AppException
{
    public override int StatusCode => 400;

    public BadRequestException(string message)
        : base(message, "BAD_REQUEST") { }
}

/// <summary>401 — Autentifikatsiya kerak.</summary>
public class UnauthorizedException : AppException
{
    public override int StatusCode => 401;

    public UnauthorizedException(string message = "Autentifikatsiya kerak")
        : base(message, "UNAUTHORIZED") { }
}

/// <summary>403 — Ruxsat yo'q.</summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => 403;

    public ForbiddenException(string message = "Bu amalga ruxsat yo'q")
        : base(message, "FORBIDDEN") { }
}

/// <summary>409 — Konflikt (band email, dublikat, race condition).</summary>
public class ConflictException : AppException
{
    public override int StatusCode => 409;

    public ConflictException(string message)
        : base(message, "CONFLICT") { }
}

/// <summary>422 — Biznes qoidasi buzildi (stock kam, order xato).</summary>
public class BusinessException : AppException
{
    public override int StatusCode => 422;

    public BusinessException(string message)
        : base(message, "BUSINESS_RULE_VIOLATED") { }
}

/// <summary>429 — Juda ko'p so'rov.</summary>
public class TooManyRequestsException : AppException
{
    public override int StatusCode => 429;

    public TooManyRequestsException(string message = "Juda ko'p so'rov, keyinroq uring")
        : base(message, "TOO_MANY_REQUESTS") { }
}
