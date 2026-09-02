using EnterpriseAiPortfolio.Ai;

namespace EnterpriseAiPortfolio.Agents;

public interface IConversationHistoryCompactor
{
    void Compact(List<AiConversationMessage> conversationHistory);
}
