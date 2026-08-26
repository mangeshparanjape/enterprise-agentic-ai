using System.ComponentModel;
using EnterpriseAiPortfolio.Services;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Plugins;

public sealed class AzureOperationsPlugin
{
    private readonly IOperationApprovalService _approvalService;

    public AzureOperationsPlugin(IOperationApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [KernelFunction]
    [Description("Creates a human approval request for restarting an Azure resource. This function never performs the restart itself.")]
    public OperationApprovalRequest RequestRestartAsync(
        [Description("Exact Azure resource name that should be restarted.")] string resourceName,
        [Description("Evidence-based reason the restart is being proposed.")] string reason)
    {
        return _approvalService.RequestRestart(resourceName, reason);
    }
}
