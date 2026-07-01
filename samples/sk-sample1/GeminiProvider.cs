using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

public class GeminiProvider : IAiProvider
{
    public PromptExecutionSettings CreateExecutionSettings()
    {
        return new GeminiPromptExecutionSettings
        {
            FunctionChoiceBehavior =
                FunctionChoiceBehavior.Auto()
        };
    }
}