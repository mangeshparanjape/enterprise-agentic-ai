using System.ComponentModel;
using EnterpriseAiPortfolio.Services;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Plugins;

public sealed class KqlInvestigationPlugin
{
    private readonly IKqlQueryService _kqlQueryService;

    public KqlInvestigationPlugin(IKqlQueryService kqlQueryService)
    {
        _kqlQueryService = kqlQueryService;
    }

    [KernelFunction]
    [Description("Performs a read-only operational log investigation for an Azure resource and returns the generated KQL, representative findings, and a concise diagnosis.")]
    public Task<KqlInvestigationResult> InvestigateResourceLogsAsync(
        [Description("Exact Azure resource name related to the alert or incident.")]
        string resourceName,
        [Description("What the investigation should determine, for example why requests are returning HTTP 504.")]
        string investigationGoal,
        [Description("How many minutes of recent logs to analyze. Valid range is 5 through 1440.")]
        int lookbackMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        return _kqlQueryService.InvestigateResourceAsync(
            resourceName,
            investigationGoal,
            lookbackMinutes,
            cancellationToken);
    }
}
