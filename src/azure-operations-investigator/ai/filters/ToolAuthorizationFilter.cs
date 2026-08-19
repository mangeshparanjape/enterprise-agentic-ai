using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai.Filters;

public sealed class ToolAuthorizationFilter : IFunctionInvocationFilter
{
    private readonly HashSet<string> _allowedPlugins;
    private readonly ILogger<ToolAuthorizationFilter> _logger;

    public ToolAuthorizationFilter(
        IOptions<ToolAuthorizationOptions> options,
        ILogger<ToolAuthorizationFilter> logger)
    {
        _allowedPlugins = new HashSet<string>(
            options.Value.AllowedPlugins,
            StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var pluginName = context.Function.PluginName ?? string.Empty;
        var functionName = context.Function.Name;

        if (!_allowedPlugins.Contains(pluginName))
        {
            _logger.LogWarning(
                "Blocked unauthorized AI tool invocation for {Plugin}.{Function}",
                pluginName,
                functionName);

            throw new UnauthorizedAccessException(
                $"AI tool invocation is not authorized for plugin '{pluginName}'.");
        }

        await next(context);
    }
}
