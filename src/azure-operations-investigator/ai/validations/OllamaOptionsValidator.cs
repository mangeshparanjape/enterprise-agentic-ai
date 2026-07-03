using Microsoft.Extensions.Options;

namespace EnterpriseAiPortfolio.Ai;

public sealed class OllamaOptionsValidator : IValidateOptions<OllamaOptions>
{
    public ValidateOptionsResult Validate(string? name, OllamaOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            failures.Add("Ollama endpoint is required.");
        }
        else if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _))
        {
            failures.Add("Ollama endpoint must be a valid absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(options.ModelId))
        {
            failures.Add("Ollama model ID is required.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}