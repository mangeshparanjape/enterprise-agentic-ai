using System.ComponentModel;
using EnterpriseAiPortfolio.Services;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Plugins;

public sealed class RunbookSearchPlugin
{
    private readonly IRunbookSearchService _runbookSearchService;

    public RunbookSearchPlugin(IRunbookSearchService runbookSearchService)
    {
        _runbookSearchService = runbookSearchService;
    }

    [KernelFunction]
    [Description("Searches approved operational runbooks and returns grounded troubleshooting guidance with source paths and relevance scores.")]
    public Task<IReadOnlyList<RunbookSearchResult>> SearchRunbooksAsync(
        [Description("The operational symptom, Azure service, error, or troubleshooting question to search for.")]
        string query,
        [Description("Maximum number of runbook matches to return. Valid range is 1 through 5.")]
        int maxResults = 3,
        CancellationToken cancellationToken = default)
    {
        return _runbookSearchService.SearchAsync(query, maxResults, cancellationToken);
    }
}
