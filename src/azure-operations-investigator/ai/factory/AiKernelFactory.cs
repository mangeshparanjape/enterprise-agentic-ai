using EnterpriseAiPortfolio.Plugins;
using EnterpriseAiPortfolio.Services;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPortfolio.Ai;

public sealed class AiKernelFactory : IAiKernelFactory
{
    private readonly IAlertService _alertService;

    public AiKernelFactory(IAlertService alertService)
    {
        _alertService = alertService;
    }

    public Kernel CreateKernel(IAiProvider provider)
    {
        var kernelBuilder = Kernel.CreateBuilder();

        provider.ConfigureKernel(kernelBuilder);

        kernelBuilder.Services.AddSingleton(_alertService);

        kernelBuilder.Plugins.AddFromType<AzureAlertPlugin>("azure_alerts");

        return kernelBuilder.Build();
    }
}