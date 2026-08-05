namespace EnterpriseAiPortfolio.Services;

public interface IRunbookSearchService
{
    Task<IReadOnlyList<RunbookSearchResult>> SearchAsync(
        string query,
        int maxResults = 3,
        CancellationToken cancellationToken = default);
}

public sealed record RunbookSearchResult(
    string Title,
    string Source,
    string Content,
    double RelevanceScore);
