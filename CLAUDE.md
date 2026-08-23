# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Enterprise-grade AI architecture portfolio project (see `README.md`). The main project, `src/azure-operations-investigator`, is a .NET 9 console app built around Microsoft Semantic Kernel. It routes operations questions to a pluggable AI provider (Ollama or Gemini) and exposes read-only Azure-alert investigation capabilities as model-callable tools (function calling). Alert/KQL/runbook data is currently mocked — no live Azure APIs are called yet.

There is also a standalone sample project, `samples/sk-sample1`, an earlier/simpler exploration of the same provider + plugin pattern (lights, Azure alerts). It is not wired into the main app.

## Commands

Run from `src/azure-operations-investigator/`:

```bash
dotnet build                  # build the main project
dotnet run                    # run the console app (interactive chat loop)
```

There is no `.sln` file and no test project in this repo yet — build/run each project directly via its `.csproj`.

The sample project is built/run the same way from `samples/sk-sample1/`.

### Configuration

Provider and behavior settings live in `src/azure-operations-investigator/appsettings.json`:

- `Ai:Provider` — `"Ollama"` or `"Gemini"` (anything else throws at startup).
- `Ai:Ollama` — `Endpoint` / `ModelId` (requires a local Ollama server, e.g. `http://localhost:11434`).
- `Ai:Gemini` — `ApiKey` / `ModelId`. Treat the API key as a secret — prefer environment variables or user-secrets over committing it, even though the checked-in value is a placeholder.
- `Ai:ToolAuthorization:AllowedPlugins` — deny-by-default allowlist of plugin names the model is permitted to invoke.
- `Agent:Operations:MaxHistoryTurns` — bounded in-memory conversation history (0 = stateless).

## Architecture

Full narrative documentation already exists in the main project and should be read/kept up to date alongside code changes:

- `src/azure-operations-investigator/architecture.md` — high-level flow, DI/startup, and a layer-by-layer walkthrough.
- `src/azure-operations-investigator/learning.md` — running design log explaining *why* each feature/abstraction was added, in build order. Treat this as the changelog of architectural decisions; append to it (don't rewrite history) when adding significant features.
- `docs/AzureOperationsInvestigator.md` — the tool-tiering model driving future scope: **read-only tools** (auto-executable today), **action tools** (state-changing, will require human approval), **administrative tools** (destructive, probably never exposed to the model directly).

### Request flow

```
Program.cs (console loop, Generic Host)
  -> IOperationsAgent (OperationsAgent) — owns system prompt + in-memory conversation history
  -> IAiRequestOrchestrator (AiRequestOrchestrator) — provider-neutral execution boundary
  -> IAiProviderFactory -> IAiProvider (OllamaProvider / GeminiProvider) — selected by Ai:Provider config
  -> IAiKernelFactory (AiKernelFactory) — builds a fresh Semantic Kernel per request, registers plugins + business services
  -> Semantic Kernel -> IAiRuntime (SemanticKernelRuntime) — translates app-level messages/history into SK ChatHistory
  -> Function invocation filters -> Plugins -> Business Services
```

Key boundaries to preserve when extending this:

- **Agent layer** (`agents/`) doesn't know which provider is active and never touches Semantic Kernel directly.
- **Orchestration layer** (`orchestration/`) is the provider-neutral seam; it maps `AiRequestContext` <-> `AiProviderRequest`/`AiProviderResponse`/`AiExecutionResult`.
- **Provider layer** (`ai/providers/`) is the only place that configures SK connectors; both providers follow the same 7-step pattern (configure connector -> create kernel -> resolve chat service -> build ChatHistory -> enable auto function calling -> get response -> map to `AiProviderResponse`).
- **Semantic Kernel is fully isolated inside `SemanticKernelRuntime`** — application code elsewhere uses `AiConversationMessage`, not SK chat types.
- **Plugins** (`plugins/`) are the model-callable tool surface and contain no data access logic themselves — they delegate to an `I*Service` in `services/`, which is where mocks get swapped for real Azure integrations later.

### Function-calling governance (order matters)

Two `IFunctionInvocationFilter`s are registered on every kernel, in this order:

1. `ToolInvocationAuditFilter` (`ai/filters/`) — logs plugin/function name, timing, and success/failure for every tool call. Never logs arguments or results (avoids leaking operational data/PII).
2. `ToolAuthorizationFilter` (`ai/filters/`) — deny-by-default enforcement against `Ai:ToolAuthorization:AllowedPlugins`; unlisted plugins throw `UnauthorizedAccessException`. Auditing runs first so denied attempts are still recorded.

When adding a new plugin, it will be blocked from execution until added to `AllowedPlugins` — this is intentional. Per `docs/AzureOperationsInvestigator.md`, any future state-changing ("action") tool should live in its own plugin so it can be gated behind an explicit approval policy rather than inheriting the current read-only allowlist.

### Adding a new AI provider

Implement `IAiProvider`, register it in `AiServiceCollectionExtensions`, and extend `AiProviderFactory`'s switch on `Ai:Provider`.

### Known inconsistency

Some model/interface files use the global namespace while most implementation files use `EnterpriseAiPortfolio.*` namespaces — be aware of this when adding new files near existing ones (match the surrounding file's convention rather than assuming one project-wide style).
