namespace EnterpriseAiPortfolio.Services;

public interface IKqlQueryService
{
    Task<KqlInvestigationResult> InvestigateResourceAsync(
        string resourceName,
        string investigationGoal,
        int lookbackMinutes,
        CancellationToken cancellationToken = default);
}

public sealed record KqlInvestigationResult(
    string ResourceName,
    string InvestigationGoal,
    int LookbackMinutes,
    string Query,
    IReadOnlyList<KqlLogFinding> Findings,
    string Summary);

public sealed record KqlLogFinding(
    DateTimeOffset Timestamp,
    string Severity,
    string Operation,
    string Message,
    string CorrelationId);
