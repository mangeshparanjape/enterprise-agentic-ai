using EnterpriseAiPortfolio.Ai.Filters;
using EnterpriseAiPortfolio.Plugins;
using EnterpriseAiPortfolio.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPortfolio.Ai;

public sealed class AiKernelFactory : IAiKernelFactory
{
    private readonly IAlertService _alertService;
    private readonly IKqlQueryService _kqlQueryService;
    private readonly IRunbookSearchService _runbookSearchService;
    private readonly ILoggerFactory _loggerFactory;

    public AiKernelFactory(
        IAlertService alertService,
        IKqlQueryService kqlQueryService,
        IRunbookSearchService runbookSearchService,
        ILoggerFactory loggerFactory)
    {
        _alertService = alertService;
        _kqlQueryService = kqlQueryService;
        _runbookSearchService = runbookSearchService;
        _loggerFactory = loggerFactory;
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

        var kernel = kernelBuilder.Build();
        kernel.FunctionInvocationFilters.Add(
            new ToolInvocationAuditFilter(
                _loggerFactory.CreateLogger<ToolInvocationAuditFilter>()));

        return kernel;
    }
}
