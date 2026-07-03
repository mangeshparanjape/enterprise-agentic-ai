using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;

public sealed class OllamaProvider : IAiProvider
{
    public void Configure(IKernelBuilder builder)
    {
        builder.AddOllamaChatCompletion(
            modelId: "llama3.2",
            endpoint: new Uri("http://localhost:11434"));
    }

    public PromptExecutionSettings CreateExecutionSettings()
    {
        return new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }
}