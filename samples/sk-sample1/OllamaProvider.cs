using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;

public class OllamaProvider : IAiProvider
{
    public PromptExecutionSettings CreateExecutionSettings()
    {
        return new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior =
                FunctionChoiceBehavior.Auto()
        };
    }
}