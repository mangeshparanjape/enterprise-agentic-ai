using Microsoft.SemanticKernel;

public interface IAiProvider
{
    PromptExecutionSettings CreateExecutionSettings();
}