using FluentValidation;
using KutubxonaAPI.DTOs.Reviews;

namespace KutubxonaAPI.Validators.Reviews;

public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Sharh matni shart")
            .Length(3, 1000).WithMessage("Sharh 3-1000 belgi bo'lishi kerak");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Reyting 1 va 5 oralig'ida bo'lishi kerak");
    }
}
