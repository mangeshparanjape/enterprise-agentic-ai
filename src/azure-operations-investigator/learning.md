# Objective

We are continuing to build an Enterprise AI Portfolio project in C# using Semantic Kernel. This project is focused on enterprise architecture and production-quality design, not quick demos.

Current architecture:

- Generic Host
- Semantic Kernel
- IAiProvider abstraction
- IAiProviderFactory
- IAiKernelFactory
- IAiRuntime
- SemanticKernelRuntime
- OllamaProvider
- GeminiProvider
- OperationsAgent
- IAiRequestOrchestrator
- AiRequestOrchestrator
- AzureAlertPlugin
- IAlertService
- MockAlertService
- Strongly typed Options (OllamaOptions, GeminiOptions, OperationsAgentOptions)
- Options Validators (IValidateOptions)
- Function calling working end-to-end
- Request/Response contracts:
  - AiRequestContext
  - AiExecutionResult
  - AiProviderRequest
  - AiProviderResponse
  - AiConversationMessage

Current execution flow:

Program.cs
→ OperationsAgent
→ AiRequestOrchestrator
→ IAiProviderFactory
→ IAiProvider
→ IAiRuntime
→ SemanticKernelRuntime
→ IAiKernelFactory
→ Semantic Kernel
→ Plugins
→ Business Services

Guiding principles:

- Teach enterprise architecture while we build.
- Explain the "why" behind every design decision.
- Build incrementally.
- One improvement at a time.
- Test after every increment.
- Plan before implementing.
- Review architecture before introducing new abstractions.
- Avoid over-engineering and premature optimization.
- Use common enterprise .NET and Semantic Kernel patterns.
- One file at a time.
- One code block per file.
- Don't skip steps or assume code.
- Challenge the architecture if something can be designed better.

## Recent feature: Conversation history

The OperationsAgent maintains in-memory conversation history and passes it through the orchestration layer into the AI runtime.

This matters because an operations assistant should support follow-up questions such as:

- "Show me the active alerts."
- "Now explain the critical one."
- "What would you check next?"

The implementation keeps Semantic Kernel isolated inside `SemanticKernelRuntime`. The application-level request model uses `AiConversationMessage` instead of exposing Semantic Kernel chat types outside the runtime boundary.

Design decision:

- `OperationsAgent` owns short-lived in-memory conversation state.
- `AiRequestContext` carries application-level history.
- `AiProviderRequest` carries provider/runtime-level history.
- `SemanticKernelRuntime` translates application messages into Semantic Kernel `ChatHistory`.

## Recent feature: Bounded conversation history

Conversation history is now limited by the configurable `Agent:Operations:MaxHistoryTurns` setting. A turn consists of one user message and one assistant response.

After each successful response, `OperationsAgent` removes the oldest completed turns when the configured limit is exceeded. The default is ten turns.

Why this matters:

- Prevents unbounded in-memory growth.
- Reduces the amount of historical context sent to the model.
- Helps control latency and token consumption as conversations grow.
- Keeps the policy configurable without coupling it to Semantic Kernel.

Setting `MaxHistoryTurns` to zero makes each request stateless while preserving the same agent implementation.

Next likely improvement:

Introduce a mocked KQL plugin so the agent can combine alert retrieval with operational log analysis.
