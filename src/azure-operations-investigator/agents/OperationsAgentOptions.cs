namespace EnterpriseAiPortfolio.Agents;

public sealed class OperationsAgentOptions
{
    public const string SectionName = "Agent:Operations";

    public int MaxHistoryTurns { get; init; } = 10;
}
