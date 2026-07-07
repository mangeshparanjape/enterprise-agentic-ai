namespace EnterpriseAiPortfolio.Ai;

public interface IAiRuntime
{
    Task<AiProviderResponse> ExecuteAsync(
        IAiProvider provider,
        AiProviderRequest request,
        CancellationToken cancellationToken = default);
}