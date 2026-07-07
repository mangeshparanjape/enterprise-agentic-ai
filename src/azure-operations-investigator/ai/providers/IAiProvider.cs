using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai;

public interface IAiProvider
{
    string Name { get; }

    void ConfigureKernel(IKernelBuilder kernelBuilder);

}