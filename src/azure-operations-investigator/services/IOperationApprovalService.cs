namespace EnterpriseAiPortfolio.Services;

public interface IOperationApprovalService
{
    OperationApprovalRequest RequestRestart(string resourceName, string reason);
    IReadOnlyCollection<OperationApprovalRequest> GetPending();
    OperationApprovalRequest ApproveAndExecute(string approvalId);
    OperationApprovalRequest Reject(string approvalId);
}

public sealed record OperationApprovalRequest(
    string Id,
    string Operation,
    string ResourceName,
    string Reason,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ResolvedAtUtc = null,
    string? Result = null);
