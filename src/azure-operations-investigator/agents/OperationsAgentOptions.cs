namespace EnterpriseAiPortfolio.Agents;

public sealed class OperationsAgentOptions
{
    public const string SectionName = "Agent:Operations";

    public int MaxHistoryMessages { get; init; } = 20;
}
