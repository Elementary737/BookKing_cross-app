using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Api.Validation;

public sealed class ValidationFilter<T>(ILogger<ValidationFilter<T>> logger) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            logger.LogWarning("400 Bad Request: Тіло запиту порожнє або має невірний формат.");
            return TypedResults.BadRequest("Тіло запиту порожнє або має невірний формат.");
        }

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        if (!isValid)
        {
            var errors = results
                .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : [string.Empty])
                    .Select(member => (member, error: r.ErrorMessage ?? "Помилка валідації")))
                .GroupBy(x => x.member)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.error).Distinct().ToArray());

            var details = string.Join("; ", errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));
            logger.LogWarning("400 Bad Request (ValidationProblem): {Details}", details);

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}