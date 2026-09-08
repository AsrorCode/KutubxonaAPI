using FluentValidation;
using KutubxonaAPI.DTOs.Books;

namespace KutubxonaAPI.Validators.Books;

public class BookCreateDtoValidator : AbstractValidator<BookCreateDto>
{
    public BookCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Kitob nomi kiritilishi shart")
            .Length(2, 200).WithMessage("Kitob nomi 2-200 belgi bo'lishi kerak");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Muallif kiritilishi shart")
            .Length(2, 150).WithMessage("Muallif 2-150 belgi bo'lishi kerak");

        RuleFor(x => x.Year)
            .InclusiveBetween(1000, 2100)
            .WithMessage("Yil 1000-2100 oralig'ida bo'lishi kerak")
            .When(x => x.Year.HasValue);

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Kategoriya 50 belgidan oshmasin");
    }
}

public class BookUpdateDtoValidator : AbstractValidator<BookUpdateDto>
{
    public BookUpdateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(2, 200);
        RuleFor(x => x.Author).NotEmpty().Length(2, 150);
        RuleFor(x => x.Year).InclusiveBetween(1000, 2100).When(x => x.Year.HasValue);
        RuleFor(x => x.Category).MaximumLength(50);
    }
}
