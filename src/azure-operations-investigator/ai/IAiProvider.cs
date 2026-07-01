using Microsoft.SemanticKernel;

public interface IAiProvider
{
    void Configure(IKernelBuilder builder);

    PromptExecutionSettings CreateExecutionSettings();
}