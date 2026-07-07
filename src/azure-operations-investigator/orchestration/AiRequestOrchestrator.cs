using System.Diagnostics;
using EnterpriseAiPortfolio.Ai;
using Microsoft.Extensions.Logging;

namespace EnterpriseAiPortfolio.Orchestration;

public sealed class AiRequestOrchestrator : IAiRequestOrchestrator
{
    private readonly IAiProviderFactory _providerFactory;
    private readonly IAiRuntime _runtime;
    private readonly ILogger<AiRequestOrchestrator> _logger;

    public AiRequestOrchestrator(
        IAiProviderFactory providerFactory,
        IAiRuntime runtime,
        ILogger<AiRequestOrchestrator> logger)
    {
        _providerFactory = providerFactory;
        _runtime = runtime;
        _logger = logger;
    }

    public async Task<AiExecutionResult> ExecuteAsync(
        AiRequestContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var provider = _providerFactory.CreateProvider();

        _logger.LogInformation(
            "AI request execution started. CorrelationId={CorrelationId}, Provider={ProviderName}, Agent={AgentName}",
            context.CorrelationId,
            provider.Name,
            context.AgentName);

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

            stopwatch.Stop();

            _logger.LogInformation(
                "AI request execution completed. CorrelationId={CorrelationId}, Provider={ProviderName}, Success={Success}, ElapsedMilliseconds={ElapsedMilliseconds}",
                context.CorrelationId,
                providerResponse.ProviderName,
                providerResponse.Success,
                stopwatch.ElapsedMilliseconds);

            return new AiExecutionResult
            {
                Response = providerResponse.Content,
                ProviderName = providerResponse.ProviderName,
                CorrelationId = context.CorrelationId,
                Success = providerResponse.Success,
                ErrorMessage = providerResponse.ErrorMessage,
                Metadata = new Dictionary<string, object>(providerResponse.Metadata)
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "AI request execution failed. CorrelationId={CorrelationId}, Provider={ProviderName}, ElapsedMilliseconds={ElapsedMilliseconds}",
                context.CorrelationId,
                provider.Name,
                stopwatch.ElapsedMilliseconds);

            return new AiExecutionResult
            {
                Response = string.Empty,
                ProviderName = provider.Name,
                CorrelationId = context.CorrelationId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
