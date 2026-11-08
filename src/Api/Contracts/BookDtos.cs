using FluentValidation;
namespace Api.Contracts;

public sealed record BookDto(string Id, string Isbn, string Title, string Author, int Year, int Copies);

public sealed record SaveBookRequest(string Isbn, string Title, string Author, int Year, int Copies);
// Додаткове 10.2
public class SaveBookRequestValidator : AbstractValidator<SaveBookRequest>
{
    public SaveBookRequestValidator()
    {
        RuleFor(x => x.Isbn).NotEmpty().WithMessage("ISBN обов'язковий.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).WithMessage("Назва обов'язкова (до 200 символів).");
        RuleFor(x => x.Author).NotEmpty().MaximumLength(100).WithMessage("Автор обов'язковий (до 100 символів).");
        RuleFor(x => x.Year).InclusiveBetween(1450, DateTime.UtcNow.Year).WithMessage($"Рік має бути в межах 1450–{DateTime.UtcNow.Year}.");
        RuleFor(x => x.Copies).GreaterThanOrEqualTo(0).WithMessage("Кількість примірників не може бути від'ємною.");
    }
}