using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai.Filters;

/// <summary>
/// Provides centralized audit telemetry around every Semantic Kernel function invocation.
/// </summary>
public sealed class ToolInvocationAuditFilter(
    ILogger<ToolInvocationAuditFilter> logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var pluginName = context.Function.PluginName;
        var functionName = context.Function.Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "AI tool invocation started: {PluginName}.{FunctionName}",
            pluginName,
            functionName);

        try
        {
            await next(context);

            stopwatch.Stop();
            logger.LogInformation(
                "AI tool invocation completed: {PluginName}.{FunctionName} in {ElapsedMilliseconds} ms",
                pluginName,
                functionName,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(
                exception,
                "AI tool invocation failed: {PluginName}.{FunctionName} after {ElapsedMilliseconds} ms",
                pluginName,
                functionName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
