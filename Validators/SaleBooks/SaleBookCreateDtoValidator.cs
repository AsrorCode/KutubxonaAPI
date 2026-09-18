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

        // ImageUrl — base64/URL. Cheksiz emas: juda katta rasm bazani shishiradi.
        // ~4 mln belgi ≈ 3MB rasm — professional chegara.
        RuleFor(x => x.ImageUrl)
            .Must(img => string.IsNullOrEmpty(img) || img.Length <= 4_000_000)
            .WithMessage("Rasm hajmi juda katta (taxminan 3MB dan oshmasin)");

        RuleFor(x => x.Category).MaximumLength(50);
        RuleFor(x => x.Year).InclusiveBetween(1000, 2100).When(x => x.Year.HasValue);

        // ===== Yangi tafsilot maydonlari =====
        RuleFor(x => x.Publisher).MaximumLength(150);
        RuleFor(x => x.CoverType).MaximumLength(50);
        RuleFor(x => x.Isbn).MaximumLength(50);
        RuleFor(x => x.Discount).InclusiveBetween(0, 100);
        RuleFor(x => x.PageCount)
            .InclusiveBetween(1, 100_000).When(x => x.PageCount.HasValue)
            .WithMessage("Sahifalar soni 1 dan 100000 gacha bo'lishi kerak");
    }
}
