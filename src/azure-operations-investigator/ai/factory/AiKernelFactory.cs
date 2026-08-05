using EnterpriseAiPortfolio.Plugins;
using EnterpriseAiPortfolio.Services;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPortfolio.Ai;

public sealed class AiKernelFactory : IAiKernelFactory
{
    private readonly IAlertService _alertService;
    private readonly IKqlQueryService _kqlQueryService;
    private readonly IRunbookSearchService _runbookSearchService;

    public AiKernelFactory(
        IAlertService alertService,
        IKqlQueryService kqlQueryService,
        IRunbookSearchService runbookSearchService)
    {
        _alertService = alertService;
        _kqlQueryService = kqlQueryService;
        _runbookSearchService = runbookSearchService;
    }

    public Kernel CreateKernel(IAiProvider provider)
    {
        var kernelBuilder = Kernel.CreateBuilder();

        provider.ConfigureKernel(kernelBuilder);

        kernelBuilder.Services.AddSingleton(_alertService);
        kernelBuilder.Services.AddSingleton(_kqlQueryService);
        kernelBuilder.Services.AddSingleton(_runbookSearchService);

        kernelBuilder.Plugins.AddFromType<AzureAlertPlugin>("azure_alerts");
        kernelBuilder.Plugins.AddFromType<KqlInvestigationPlugin>("kql_investigation");
        kernelBuilder.Plugins.AddFromType<RunbookSearchPlugin>("runbook_search");

        return kernelBuilder.Build();
    }
}
