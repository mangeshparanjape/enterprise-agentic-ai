using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai;

public interface IAiKernelFactory
{
    Kernel CreateKernel(IAiProvider provider);
}