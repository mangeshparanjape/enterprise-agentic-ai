namespace EnterpriseAiPortfolio.Agents;

public sealed class OperationsAgentOptions
{
    public const string SectionName = "Agent:Operations";

    public int MaxHistoryTurns { get; init; } = 10;

    public int MaxHistoryTokens { get; init; } = 6000;

    public int PreserveRecentTurns { get; init; } = 2;
}
