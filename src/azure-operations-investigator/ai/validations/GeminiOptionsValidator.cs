using Microsoft.Extensions.Options;

namespace EnterpriseAiPortfolio.Ai;

public sealed class GeminiOptionsValidator : IValidateOptions<GeminiOptions>
{
    public ValidateOptionsResult Validate(string? name, GeminiOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            failures.Add("Gemini API key is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ModelId))
        {
            failures.Add("Gemini model ID is required.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}