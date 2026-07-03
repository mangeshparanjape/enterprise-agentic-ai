using System.ComponentModel;
using Microsoft.SemanticKernel;

public sealed class AzureAlertPlugin
{
    private readonly IAlertService _alertService;

    public AzureAlertPlugin(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [KernelFunction]
    [Description("Gets read-only Azure alert details, affected resource, severity, signal, and recommended investigation steps.")]
    public Task<AlertDetails> GetAlertDetailsAsync(
        [Description("Azure alert identifier, for example A123.")]
        string alertId)
    {
        return _alertService.GetAlertDetailsAsync(alertId);
    }
}