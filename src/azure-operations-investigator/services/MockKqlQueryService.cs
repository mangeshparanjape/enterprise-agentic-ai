namespace EnterpriseAiPortfolio.Services;

public sealed class MockKqlQueryService : IKqlQueryService
{
    public Task<KqlInvestigationResult> InvestigateResourceAsync(
        string resourceName,
        string investigationGoal,
        int lookbackMinutes,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(investigationGoal);

        if (lookbackMinutes is < 5 or > 1440)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackMinutes),
                "Lookback must be between 5 minutes and 24 hours.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var query = $$"""
            AzureDiagnostics
            | where TimeGenerated >= ago({{lookbackMinutes}}m)
            | where Resource == "{{resourceName}}"
            | where Category == "GatewayLogs"
            | project TimeGenerated, Level, OperationName, Message, CorrelationId
            | order by TimeGenerated desc
            | take 20
            """;

        var now = DateTimeOffset.UtcNow;
        IReadOnlyList<KqlLogFinding> findings =
        [
            new(
                now.AddMinutes(-12),
                "Error",
                "BackendRequest",
                "Backend connection timed out after 9.8 seconds.",
                "corr-7f31"),
            new(
                now.AddMinutes(-13),
                "Warning",
                "PolicyExecution",
                "Retry policy exhausted after two attempts.",
                "corr-7f31"),
            new(
                now.AddMinutes(-18),
                "Information",
                "GatewayRequest",
                "Request completed with status code 504.",
                "corr-51a2")
        ];

        var result = new KqlInvestigationResult(
            resourceName,
            investigationGoal,
            lookbackMinutes,
            query,
            findings,
            "The mock logs show repeated backend timeouts and exhausted retries. Investigate backend reachability, latency, and recent network changes before scaling the gateway.");

        return Task.FromResult(result);
    }
}
