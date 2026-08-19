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
- RunbookSearchPlugin
- IAlertService
- MockAlertService
- IKqlQueryService
- MockKqlQueryService
- IRunbookSearchService
- MockRunbookSearchService
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

Approved plugin names are configured under `Ai:ToolAuthorization:AllowedPlugins`. The current read-only plugins are explicitly allowed:

- `azure_alerts`
- `kql_investigation`
- `runbook_search`

Any plugin that is registered later but not explicitly added to the allowlist is blocked with `UnauthorizedAccessException`.

Design decisions:

- Authorization is enforced in code, not through system-prompt instructions.
- The policy is deny-by-default so adding a new plugin does not automatically grant the model execution rights.
- Configuration is validated at startup to prevent an accidentally empty or malformed policy.
- The audit filter is registered before the authorization filter so denied invocation attempts are still recorded as failed tool calls.
- Plugin arguments and results remain outside authorization logs.
- Future state-changing capabilities should be placed in dedicated action plugins so they can remain denied until an approval/authorization policy is intentionally added.

Why this matters:

Function calling gives the model the ability to select application capabilities, but the application must remain the final authority over what can execute. This establishes a least-privilege boundary before any state-changing operations tools are introduced.

Next likely improvement:

Add a state-changing operations tool behind an explicit human-approval policy, while keeping the current read-only tools automatically executable.
