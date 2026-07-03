using EnterpriseAiPortfolio.Ai;

namespace EnterpriseAiPortfolio.Orchestration;

public interface IAiRequestOrchestrator
{
    Task<AiExecutionResult> ExecuteAsync(
        AiRequestContext context,
        CancellationToken cancellationToken = default);
}