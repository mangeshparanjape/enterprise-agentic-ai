using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai;

public interface IAiProvider
{
    string Name { get; }

    void ConfigureKernel(IKernelBuilder kernelBuilder);

    Task<AiProviderResponse> ExecuteAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default);
}