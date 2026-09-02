using EnterpriseAiPortfolio.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPortfolio.Agents;

public sealed class TokenAwareConversationHistoryCompactor : IConversationHistoryCompactor
{
    private const int ApproximateCharactersPerToken = 4;
    private const int ApproximateMessageOverheadTokens = 4;

    private readonly OperationsAgentOptions _options;
    private readonly ILogger<TokenAwareConversationHistoryCompactor> _logger;

    public TokenAwareConversationHistoryCompactor(
        IOptions<OperationsAgentOptions> options,
        ILogger<TokenAwareConversationHistoryCompactor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public void Compact(List<AiConversationMessage> conversationHistory)
    {
        if (_options.MaxHistoryTurns == 0)
        {
            conversationHistory.Clear();
            return;
        }

        var beforeMessages = conversationHistory.Count;
        var beforeTokens = EstimateTokens(conversationHistory);

        EnforceTurnLimit(conversationHistory);
        EnforceTokenBudget(conversationHistory);

        if (conversationHistory.Count != beforeMessages)
        {
            _logger.LogInformation(
                "Conversation history compacted from {BeforeMessages} to {AfterMessages} messages; estimated tokens {BeforeTokens} -> {AfterTokens}.",
                beforeMessages,
                conversationHistory.Count,
                beforeTokens,
                EstimateTokens(conversationHistory));
        }
    }

    private void EnforceTurnLimit(List<AiConversationMessage> conversationHistory)
    {
        var maxMessages = _options.MaxHistoryTurns * 2;
        RemoveOldestCompleteTurns(conversationHistory, conversationHistory.Count - maxMessages);
    }

    private void EnforceTokenBudget(List<AiConversationMessage> conversationHistory)
    {
        if (_options.MaxHistoryTokens == 0)
        {
            return;
        }

        var minimumMessagesToPreserve = Math.Min(
            conversationHistory.Count,
            _options.PreserveRecentTurns * 2);

        while (conversationHistory.Count > minimumMessagesToPreserve &&
               EstimateTokens(conversationHistory) > _options.MaxHistoryTokens)
        {
            RemoveOldestCompleteTurns(conversationHistory, 2);
        }
    }

    private static void RemoveOldestCompleteTurns(
        List<AiConversationMessage> conversationHistory,
        int requestedMessagesToRemove)
    {
        if (requestedMessagesToRemove <= 0 || conversationHistory.Count < 2)
        {
            return;
        }

        var messagesToRemove = Math.Min(requestedMessagesToRemove, conversationHistory.Count);

        if (messagesToRemove % 2 != 0)
        {
            messagesToRemove++;
        }

        messagesToRemove = Math.Min(messagesToRemove, conversationHistory.Count - conversationHistory.Count % 2);

        if (messagesToRemove > 0)
        {
            conversationHistory.RemoveRange(0, messagesToRemove);
        }
    }

    private static int EstimateTokens(IEnumerable<AiConversationMessage> conversationHistory)
    {
        return conversationHistory.Sum(message =>
            (int)Math.Ceiling(message.Content.Length / (double)ApproximateCharactersPerToken) +
            ApproximateMessageOverheadTokens);
    }
}
