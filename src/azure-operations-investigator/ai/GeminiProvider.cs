using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

public sealed class GeminiProvider : IAiProvider
{
    public void Configure(IKernelBuilder builder)
    {
        builder.AddGoogleAIGeminiChatCompletion(
            modelId: "...",
            apiKey: "...",
            apiVersion: GoogleAIVersion.V1_Beta);
    }

    public PromptExecutionSettings CreateExecutionSettings()
    {
        return new GeminiPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }
}