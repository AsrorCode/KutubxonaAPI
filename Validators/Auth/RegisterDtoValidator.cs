using FluentValidation;
using KutubxonaAPI.Controllers;

namespace KutubxonaAPI.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email kerak")
            .EmailAddress().WithMessage("Email format noto'g'ri")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parol kerak")
            .MinimumLength(8).WithMessage("Parol kamida 8 belgi")
            .MaximumLength(100).WithMessage("Parol 100 belgidan oshmasin")
            .Matches("[A-Z]").WithMessage("Parolda kamida 1 katta harf bo'lishi kerak")
            .Matches("[a-z]").WithMessage("Parolda kamida 1 kichik harf bo'lishi kerak")
            .Matches("[0-9]").WithMessage("Parolda kamida 1 raqam bo'lishi kerak")
            .Must(password => !password.Contains(' '))
                .WithMessage("Parolda bo'sh joy bo'lmasin");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ism kerak")
            .Length(2, 50).WithMessage("Ism 2-50 belgi bo'lsin");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Familiya kerak")
            .Length(2, 50).WithMessage("Familiya 2-50 belgi bo'lsin");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
