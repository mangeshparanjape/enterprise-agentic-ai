using EnterpriseAiPortfolio.Ai.Filters;
using EnterpriseAiPortfolio.Plugins;
using EnterpriseAiPortfolio.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPortfolio.Ai;

public sealed class AiKernelFactory : IAiKernelFactory
{
    private readonly IAlertService _alertService;
    private readonly IKqlQueryService _kqlQueryService;
    private readonly IRunbookSearchService _runbookSearchService;
    private readonly IOperationApprovalService _operationApprovalService;
    private readonly IOptions<ToolAuthorizationOptions> _toolAuthorizationOptions;
    private readonly ILoggerFactory _loggerFactory;

    public AiKernelFactory(
        IAlertService alertService,
        IKqlQueryService kqlQueryService,
        IRunbookSearchService runbookSearchService,
        IOperationApprovalService operationApprovalService,
        IOptions<ToolAuthorizationOptions> toolAuthorizationOptions,
        ILoggerFactory loggerFactory)
    {
        _alertService = alertService;
        _kqlQueryService = kqlQueryService;
        _runbookSearchService = runbookSearchService;
        _operationApprovalService = operationApprovalService;
        _toolAuthorizationOptions = toolAuthorizationOptions;
        _loggerFactory = loggerFactory;
    }

    public Kernel CreateKernel(IAiProvider provider)
    {
        var kernelBuilder = Kernel.CreateBuilder();

        provider.ConfigureKernel(kernelBuilder);

        kernelBuilder.Services.AddSingleton(_alertService);
        kernelBuilder.Services.AddSingleton(_kqlQueryService);
        kernelBuilder.Services.AddSingleton(_runbookSearchService);
        kernelBuilder.Services.AddSingleton(_operationApprovalService);

        kernelBuilder.Plugins.AddFromType<AzureAlertPlugin>("azure_alerts");
        kernelBuilder.Plugins.AddFromType<KqlInvestigationPlugin>("kql_investigation");
        kernelBuilder.Plugins.AddFromType<RunbookSearchPlugin>("runbook_search");
        kernelBuilder.Plugins.AddFromType<AzureOperationsPlugin>("azure_operations");

        var kernel = kernelBuilder.Build();

        // Audit wraps authorization so denied attempts are logged as failed invocations.
        kernel.FunctionInvocationFilters.Add(
            new ToolInvocationAuditFilter(
                _loggerFactory.CreateLogger<ToolInvocationAuditFilter>()));
        kernel.FunctionInvocationFilters.Add(
            new ToolAuthorizationFilter(
                _toolAuthorizationOptions,
                _loggerFactory.CreateLogger<ToolAuthorizationFilter>()));

        return kernel;
    }
}
