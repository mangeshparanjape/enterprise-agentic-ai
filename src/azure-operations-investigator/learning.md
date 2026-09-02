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
- IConversationHistoryCompactor
- TokenAwareConversationHistoryCompactor
- IAiRequestOrchestrator
- AiRequestOrchestrator
- AzureAlertPlugin
- KqlInvestigationPlugin
- RunbookSearchPlugin
- AzureOperationsPlugin
- IAlertService
- MockAlertService
- IKqlQueryService
- MockKqlQueryService
- IRunbookSearchService
- MockRunbookSearchService
- IOperationApprovalService
- InMemoryOperationApprovalService
- ToolInvocationAuditFilter
- ToolAuthorizationFilter
- Strongly typed Options (OllamaOptions, GeminiOptions, OperationsAgentOptions, ToolAuthorizationOptions)
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
→ Function invocation filters
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

## Recent feature: Tool invocation audit filter

Every Semantic Kernel function invocation now passes through `ToolInvocationAuditFilter`, an `IFunctionInvocationFilter` registered at the kernel boundary.

The filter records:

- Plugin and function name when execution starts.
- Successful completion and elapsed milliseconds.
- Exceptions, plugin/function identity, and elapsed milliseconds when execution fails.

Design decisions:

- Auditing is centralized instead of duplicated inside every plugin.
- Plugin arguments and results are intentionally not logged, reducing the risk of leaking sensitive operational data or PII.
- The filter uses the Generic Host `ILoggerFactory`, so tool telemetry follows the application's normal logging providers and configuration.
- Exceptions are logged and rethrown so the filter does not change business behavior.
- Registering the filter after the kernel is built makes filter ordering explicit for future authorization, redaction, or approval filters.

Why this matters:

Function calling lets the model choose application capabilities. Enterprise systems need an application-controlled interception point around those calls for observability and, later, policy enforcement. Semantic Kernel filters provide that boundary without coupling governance logic to individual tools.

## Recent feature: Deny-by-default tool authorization

`ToolAuthorizationFilter` now enforces an application-controlled allowlist before a Semantic Kernel plugin function can execute.

Approved plugin names are configured under `Ai:ToolAuthorization:AllowedPlugins`. The approved plugins are explicitly listed rather than implicitly trusted.

Any plugin that is registered later but not explicitly added to the allowlist is blocked with `UnauthorizedAccessException`.

Design decisions:

- Authorization is enforced in code, not through system-prompt instructions.
- The policy is deny-by-default so adding a new plugin does not automatically grant the model execution rights.
- Configuration is validated at startup to prevent an accidentally empty or malformed policy.
- The audit filter is registered before the authorization filter so denied invocation attempts are still recorded as failed tool calls.
- Plugin arguments and results remain outside authorization logs.

Why this matters:

Function calling gives the model the ability to select application capabilities, but the application must remain the final authority over what can execute.

## Recent feature: Human approval for state-changing operations

`AzureOperationsPlugin` introduces the first state-changing operations workflow without giving the model direct execution authority.

The plugin exposes only `RequestRestartAsync`. Calling it creates a pending `OperationApprovalRequest` through `IOperationApprovalService`; it does not restart anything.

A human operator resolves the request through console commands that are deliberately outside the Semantic Kernel tool path:

- `/pending`
- `/approve <id>`
- `/reject <id>`

`InMemoryOperationApprovalService` simulates the restart only after `/approve <id>` is entered. Rejected requests are marked rejected and never execute.

Design decisions:

- The model can propose an action but cannot approve its own action.
- Approval state is owned by application code, not the prompt.
- The approval identifier binds the human decision to a specific resource, operation, and reason.
- A resolved approval cannot be reused.
- The current execution is intentionally simulated; a future Azure SDK implementation can replace it behind the same approval boundary.
- `azure_operations` is explicitly allowlisted because its exposed capability is proposal-only; direct restart execution is not a Kernel function.

Why this matters:

Authorization answers whether a capability may participate in the application. Human approval answers whether a specific consequential invocation should proceed. Keeping execution outside the model-accessible tool surface prevents the LLM from approving or invoking the protected action itself.

## Recent feature: Token-aware conversation compaction

`OperationsAgent` now delegates history management to `IConversationHistoryCompactor` instead of trimming messages itself. The initial implementation, `TokenAwareConversationHistoryCompactor`, uses a token-aware truncation strategy while preserving complete user/assistant turns.

Configuration under `Agent:Operations` now includes:

- `MaxHistoryTurns` — hard cap on retained turns; zero remains stateless mode.
- `MaxHistoryTokens` — approximate token budget for retained history; zero disables the token budget.
- `PreserveRecentTurns` — minimum number of most recent complete turns retained while applying the token budget.

The estimator intentionally uses a lightweight approximation of four characters per token plus a small per-message overhead. This avoids coupling the agent layer to any one provider tokenizer while still making context growth visible and controllable.

Design decisions:

- Compaction is an application concern and remains outside Semantic Kernel.
- The compactor removes only complete user/assistant turn pairs so history is never left structurally half-complete.
- The hard turn cap is applied first, then the token budget.
- Recent turns are protected from token-budget truncation to retain local conversational continuity.
- Compaction logs message counts and estimated token counts, but never message content.
- Startup validation rejects negative limits and configurations where `PreserveRecentTurns` exceeds `MaxHistoryTurns`.
- The abstraction allows a future summarization-based compactor or Agent Framework compaction provider to replace this implementation without changing `OperationsAgent`.

Why this matters:

A fixed turn count is a weak proxy for model context usage because one turn can contain a few words while another can contain thousands. A token-aware policy better controls cost, latency, and context-window pressure while keeping the current implementation deterministic and provider-neutral.

Next likely improvement:

Add automated unit tests for the compaction policy, then evolve from truncation to summarization once a stable provider-neutral summarization boundary is defined.