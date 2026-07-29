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
- KqlInvestigationPlugin
- IAlertService
- MockAlertService
- IKqlQueryService
- MockKqlQueryService
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

## Recent feature: Mocked KQL investigation plugin

The agent can now perform a read-only operational log investigation after retrieving an Azure alert. `KqlInvestigationPlugin` exposes a semantically described function to the model while `IKqlQueryService` owns the log-analysis capability.

The initial implementation uses `MockKqlQueryService`, which returns:

- A generated read-only KQL query scoped to a resource and lookback period.
- Representative operational findings with timestamps, severity, operation, message, and correlation ID.
- A concise diagnosis that the agent can combine with alert details.

Design decisions:

- The plugin accepts an investigation goal instead of arbitrary KQL from the model.
- The service generates the query, preserving a controlled read-only boundary.
- Lookback is constrained to 5 minutes through 24 hours.
- The service abstraction can later be replaced by an Azure Monitor or Log Analytics implementation without changing the plugin or agent.
- Cancellation is propagated through the plugin and service contract.

Example prompt:

"Investigate alert A123 and use recent logs to explain the likely cause."

Semantic Kernel can first call `azure_alerts.GetAlertDetailsAsync` and then call `kql_investigation.InvestigateResourceLogsAsync` using the affected resource from the alert.

Next likely improvement:

Add automated tests for plugin metadata, input validation, and the alert-to-log investigation workflow.
