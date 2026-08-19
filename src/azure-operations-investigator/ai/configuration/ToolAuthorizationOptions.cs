namespace EnterpriseAiPortfolio.Ai;

public sealed class ToolAuthorizationOptions
{
    public const string SectionName = "Ai:ToolAuthorization";

    public string[] AllowedPlugins { get; init; } = [];
}
