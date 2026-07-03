namespace EnterpriseAiPortfolio.Agents;

public interface IOperationsAgent
{
    Task<string> ChatAsync(
        string userMessage,
        CancellationToken cancellationToken = default);
}