using System.Text.RegularExpressions;

namespace EnterpriseAiPortfolio.Services;

public sealed class MockRunbookSearchService : IRunbookSearchService
{
    private static readonly IReadOnlyList<RunbookDocument> Documents =
    [
        new(
            "Application Gateway 502 and 504 Investigation",
            "runbooks/application-gateway-backend-errors.md",
            "Check backend health, probe status, DNS resolution, NSG and route reachability, TLS certificate trust, SNI, and the backend Host header. A 502 commonly indicates an unhealthy or unreachable backend. A 504 commonly indicates that the gateway connected but the backend did not respond before the request timeout."),
        new(
            "Azure Function Startup Dependency Failures",
            "runbooks/function-startup-dependencies.md",
            "When a Function App fails during startup, validate access to Key Vault and App Configuration, managed identity role assignments, private DNS resolution, firewall routing, and required SQL Always Encrypted keys. Review application traces for configuration-loading exceptions before investigating request handling."),
        new(
            "APIM Backend Timeout Investigation",
            "runbooks/apim-backend-timeouts.md",
            "Correlate APIM gateway logs with backend Application Insights using operation or correlation identifiers. Review backend duration, status code zero, timeout policy values, retries, network connectivity, TLS negotiation, and whether the backend received the request. Avoid retries for non-idempotent operations unless an idempotency mechanism exists."),
        new(
            "Private Endpoint DNS Troubleshooting",
            "runbooks/private-endpoint-dns.md",
            "Confirm the requesting workload resolves the service name to the expected private endpoint IP. Compare NameResolver, nslookup, and effective DNS settings. Validate private DNS zone links, custom DNS forwarders, conditional forwarding, negative caching, and routing to the resolved private IP."),
        new(
            "Mutual TLS Certificate Investigation",
            "runbooks/mtls-certificate-investigation.md",
            "Capture the TLS handshake and compare the presented client certificate, subject, issuer, serial number, validity dates, chain, and EKU with the working environment. Confirm the client sends the certificate and private key, the server trusts the issuing CA, and renewal did not change certificate-selection behavior.")
    ];

    public Task<IReadOnlyList<RunbookSearchResult>> SearchAsync(
        string query,
        int maxResults = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        if (maxResults is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxResults),
                "Maximum results must be between 1 and 5.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var queryTerms = Tokenize(query);

        IReadOnlyList<RunbookSearchResult> results = Documents
            .Select(document => new
            {
                Document = document,
                Score = CalculateScore(queryTerms, Tokenize($"{document.Title} {document.Content}"))
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Document.Title, StringComparer.Ordinal)
            .Take(maxResults)
            .Select(item => new RunbookSearchResult(
                item.Document.Title,
                item.Document.Source,
                item.Document.Content,
                Math.Round(item.Score, 3)))
            .ToArray();

        return Task.FromResult(results);
    }

    private static HashSet<string> Tokenize(string value) =>
        Regex.Matches(value.ToLowerInvariant(), "[a-z0-9]+")
            .Select(match => match.Value)
            .Where(term => term.Length > 2)
            .ToHashSet(StringComparer.Ordinal);

    private static double CalculateScore(
        IReadOnlySet<string> queryTerms,
        IReadOnlySet<string> documentTerms)
    {
        if (queryTerms.Count == 0)
        {
            return 0;
        }

        var matchingTerms = queryTerms.Count(documentTerms.Contains);
        return (double)matchingTerms / queryTerms.Count;
    }

    private sealed record RunbookDocument(string Title, string Source, string Content);
}
