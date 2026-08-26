using System.Collections.Concurrent;

namespace EnterpriseAiPortfolio.Services;

public sealed class InMemoryOperationApprovalService : IOperationApprovalService
{
    private readonly ConcurrentDictionary<string, OperationApprovalRequest> _requests = new(StringComparer.OrdinalIgnoreCase);

    public OperationApprovalRequest RequestRestart(string resourceName, string reason)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException("Resource name is required.", nameof(resourceName));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required for a state-changing operation.", nameof(reason));
        }

        var request = new OperationApprovalRequest(
            Guid.NewGuid().ToString("N"),
            "RestartResource",
            resourceName.Trim(),
            reason.Trim(),
            "Pending",
            DateTimeOffset.UtcNow);

        _requests[request.Id] = request;
        return request;
    }

    public IReadOnlyCollection<OperationApprovalRequest> GetPending() =>
        _requests.Values
            .Where(request => string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            .OrderBy(request => request.CreatedAtUtc)
            .ToArray();

    public OperationApprovalRequest ApproveAndExecute(string approvalId)
    {
        var request = GetPendingRequest(approvalId);
        var resolved = request with
        {
            Status = "ApprovedAndExecuted",
            ResolvedAtUtc = DateTimeOffset.UtcNow,
            Result = $"Simulated restart completed for '{request.ResourceName}'."
        };

        _requests[request.Id] = resolved;
        return resolved;
    }

    public OperationApprovalRequest Reject(string approvalId)
    {
        var request = GetPendingRequest(approvalId);
        var resolved = request with
        {
            Status = "Rejected",
            ResolvedAtUtc = DateTimeOffset.UtcNow,
            Result = "Operation was not executed."
        };

        _requests[request.Id] = resolved;
        return resolved;
    }

    private OperationApprovalRequest GetPendingRequest(string approvalId)
    {
        if (string.IsNullOrWhiteSpace(approvalId) ||
            !_requests.TryGetValue(approvalId.Trim(), out var request))
        {
            throw new KeyNotFoundException($"Approval request '{approvalId}' was not found.");
        }

        if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Approval request '{approvalId}' is already {request.Status}.");
        }

        return request;
    }
}
