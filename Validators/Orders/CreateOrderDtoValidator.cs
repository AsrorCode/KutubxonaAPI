using FluentValidation;
using KutubxonaAPI.Controllers;

namespace KutubxonaAPI.Validators.Orders;

/// <summary>
/// Buyurtma yaratish uchun validatsiya (FluentValidation).
/// Kontrollerdagi qo'lda tekshiruvlar o'rniga — markazlashgan qoidalar.
/// </summary>
public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Ism shart")
            .Length(2, 100);

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Telefon raqam shart")
            .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("Telefon raqam noto'g'ri");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Yetkazish manzili shart")
            .Length(5, 500);

        RuleFor(x => x.Notes).MaximumLength(1000);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Buyurtmada hech bo'lmaganda 1 ta kitob bo'lishi kerak");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.SaleBookId).GreaterThan(0);
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Miqdor 0 dan katta bo'lishi kerak")
                .LessThanOrEqualTo(1000).WithMessage("Miqdor juda katta");
        });
    }
}
