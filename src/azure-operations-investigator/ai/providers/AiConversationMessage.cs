namespace EnterpriseAiPortfolio.Ai;

public sealed class AiConversationMessage
{
    public required string Role { get; init; }

    public required string Content { get; init; }

    public static AiConversationMessage User(string content)
    {
        return new AiConversationMessage
        {
            Role = "user",
            Content = content
        };
    }

    public static AiConversationMessage Assistant(string content)
    {
        return new AiConversationMessage
        {
            Role = "assistant",
            Content = content
        };
    }
}
