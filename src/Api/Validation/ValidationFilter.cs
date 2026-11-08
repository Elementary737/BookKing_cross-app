using FluentValidation;

namespace Api.Validation;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(context);

        var argToValidate = context.Arguments.OfType<T>().FirstOrDefault();
        if (argToValidate is null) return await next(context);

        var validationResult = await validator.ValidateAsync(argToValidate);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}