using EnterpriseAiPortfolio.Ai;

namespace EnterpriseAiPortfolio.Orchestration;

public sealed class AiRequestOrchestrator : IAiRequestOrchestrator
{
    private readonly IAiProviderFactory _providerFactory;
    private readonly IAiRuntime _runtime;

    public AiRequestOrchestrator(
        IAiProviderFactory providerFactory,
        IAiRuntime runtime)
    {
        _providerFactory = providerFactory;
        _runtime = runtime;
    }

    public async Task<AiExecutionResult> ExecuteAsync(
        AiRequestContext context,
        CancellationToken cancellationToken = default)
    {
        var provider = _providerFactory.CreateProvider();

        var providerRequest = new AiProviderRequest
        {
            UserMessage = context.UserMessage,
            SystemMessage = context.SystemMessage,
            AgentName = context.AgentName,
            Metadata = new Dictionary<string, object>(context.Metadata)
        };

        try
        {
            var providerResponse = await _runtime.ExecuteAsync(
                provider,
                providerRequest,
                cancellationToken);

            return new AiExecutionResult
            {
                Response = providerResponse.Content,
                ProviderName = providerResponse.ProviderName,
                Success = providerResponse.Success,
                ErrorMessage = providerResponse.ErrorMessage,
                Metadata = new Dictionary<string, object>(providerResponse.Metadata)
            };
        }
        catch (Exception ex)
        {
            return new AiExecutionResult
            {
                Response = string.Empty,
                ProviderName = provider.Name,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}