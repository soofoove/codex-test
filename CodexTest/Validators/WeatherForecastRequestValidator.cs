using CodexTest.Models;
using FluentValidation;

namespace CodexTest.Validators;

public class WeatherForecastRequestValidator : AbstractValidator<WeatherForecastRequest>
{
    public WeatherForecastRequestValidator()
    {
        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary is required.");
    }
}
