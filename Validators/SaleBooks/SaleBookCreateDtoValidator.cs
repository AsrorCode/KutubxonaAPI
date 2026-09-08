using FluentValidation;
using KutubxonaAPI.DTOs.SaleBooks;

namespace KutubxonaAPI.Validators.SaleBooks;

public class SaleBookCreateDtoValidator : AbstractValidator<SaleBookCreateDto>
{
    public SaleBookCreateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(2, 200);
        RuleFor(x => x.Author).NotEmpty().Length(2, 150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Narx 0 dan katta bo'lishi kerak")
            .LessThan(100_000_000).WithMessage("Narx juda katta");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
        RuleFor(x => x.Category).MaximumLength(50);
        RuleFor(x => x.Year).InclusiveBetween(1000, 2100).When(x => x.Year.HasValue);
    }
}
