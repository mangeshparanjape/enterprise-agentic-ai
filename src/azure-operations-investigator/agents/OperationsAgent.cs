using EnterpriseAiPortfolio.Orchestration;

namespace EnterpriseAiPortfolio.Agents;

public sealed class OperationsAgent : IOperationsAgent
{
    private readonly IAiRequestOrchestrator _orchestrator;

    public OperationsAgent(IAiRequestOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<string> ChatAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var request = new AiRequestContext
        {
            UserMessage = userMessage,
            AgentName = nameof(OperationsAgent),
            SystemMessage = """
                You are an enterprise operations assistant.
                Use available plugins whenever appropriate to answer questions accurately.
                """
        };

        var result = await _orchestrator.ExecuteAsync(
            request,
            cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"AI request failed. Provider: {result.ProviderName}. Error: {result.ErrorMessage}");
        }

        return result.Response;
    }
}