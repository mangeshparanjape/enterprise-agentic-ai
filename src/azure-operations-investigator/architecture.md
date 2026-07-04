# Azure Operations Investigator Architecture

This project is a .NET console application that uses Semantic Kernel to route
operations questions to an AI provider and expose Azure-alert investigation
functions as model-callable tools.

The current Azure alert backend is mocked, but the architecture already separates
the console experience, agent behavior, AI provider selection, Semantic Kernel
configuration, plugin surface, and business services.

## High-Level Flow

```text
Console
  -> OperationsAgent
  -> AiRequestOrchestrator
  -> AiProviderFactory
  -> OllamaProvider / GeminiProvider
  -> AiKernelFactory
  -> Semantic Kernel
  -> AzureAlertPlugin
  -> IAlertService
```

`Program.cs` owns application startup and the console loop. It reads user input,
passes it to `IOperationsAgent`, and prints the returned assistant response.

## Startup And Dependency Injection

`Program.cs` builds a .NET Generic Host and registers the app components:

- `OllamaOptions` and `GeminiOptions` are bound from configuration and validated
  on startup.
- `IAlertService` is registered as `MockAlertService`.
- `OllamaProvider` and `GeminiProvider` are registered as concrete AI providers.
- `IAiProviderFactory` chooses the active provider from `Ai:Provider`.
- `IAiKernelFactory` creates a Semantic Kernel instance for a provider.
- `IAiRequestOrchestrator` executes provider-neutral AI requests.
- `IOperationsAgent` exposes the user-facing agent API.

Configuration lives in `appsettings.json`:

```json
{
  "Ai": {
    "Provider": "Ollama",
    "Ollama": {
      "Endpoint": "http://localhost:11434",
      "ModelId": "llama3.2"
    },
    "Gemini": {
      "ApiKey": "YOUR_API_KEY",
      "ModelId": "gemini-2.5-flash"
    }
  }
}
```

## Agent Layer

`OperationsAgent` is the domain-specific entry point. It does not know which
model provider is active and does not call Semantic Kernel directly.

Its responsibilities are:

- Accept the user message from the console.
- Build an `AiRequestContext`.
- Add the agent identity, currently `OperationsAgent`.
- Add the system prompt that frames the assistant as an enterprise operations
  assistant.
- Delegate execution to `IAiRequestOrchestrator`.
- Convert unsuccessful executions into an exception.

This keeps business intent close to the agent while leaving provider mechanics
outside the agent.

## Orchestration Layer

`AiRequestOrchestrator` is the provider-neutral execution boundary.

It receives `AiRequestContext`, asks `IAiProviderFactory` for the configured
provider, maps the context into `AiProviderRequest`, calls the provider, and maps
the response into `AiExecutionResult`.

It also catches provider exceptions and turns them into a failed
`AiExecutionResult`, preserving the provider name and error message.

## Provider Selection

`AiProviderFactory` reads `Ai:Provider` from configuration:

- `Ollama` returns `OllamaProvider`.
- `Gemini` returns `GeminiProvider`.
- Any other value throws an `InvalidOperationException`.

This makes provider switching a configuration choice instead of an agent or
orchestrator change.

## Provider Implementations

Both providers implement `IAiProvider` and follow the same pattern:

1. Configure the Semantic Kernel builder with the provider-specific chat
   completion connector.
2. Create a kernel through `IAiKernelFactory`.
3. Resolve `IChatCompletionService` from the kernel.
4. Build a `ChatHistory` with the optional system prompt and user message.
5. Enable automatic function calling.
6. Ask Semantic Kernel for a chat response.
7. Return an `AiProviderResponse`.

`OllamaProvider` configures the Ollama connector from `OllamaOptions` using
`Endpoint` and `ModelId`.

`GeminiProvider` configures the Gemini connector from `GeminiOptions` using
`ApiKey` and `ModelId`.

## Semantic Kernel Boundary

`AiKernelFactory` creates a fresh kernel for each provider execution.

The provider first configures the model connector. Then the factory adds the
business service instance and registers `AzureAlertPlugin` under the plugin name
`azure_alerts`.

This is the key integration point:

```text
AI provider connector + plugin functions + business services = executable kernel
```

Because automatic function calling is enabled, the model can choose to call
registered plugin functions when the user asks about alerts.

## Plugin Layer

`AzureAlertPlugin` is the model-callable tool surface. It exposes read-only
operations functions:

- `GetActiveAlertsAsync()` returns active alert summaries.
- `GetAlertDetailsAsync(alertId)` returns severity, resource, region, signal,
  recommended next steps, and safety information for one alert.

The plugin itself contains no alert data. It delegates all data access to
`IAlertService`.

## Business Services And Models

`IAlertService` defines the alert data boundary:

- `GetActiveAlertsAsync()`
- `GetAlertDetailsAsync(string alertId)`

`MockAlertService` currently returns hardcoded alert data:

- `A123`, `A124`, and `A125` alert summaries.
- Detailed alert data for a requested alert ID.
- Safety metadata showing the operation is `ReadOnly` and not destructive.

The alert models are simple records:

- `AlertSummary`
- `AlertDetails`
- `AlertSafety`

This layer is the natural place to replace mock data with real Azure Monitor,
Azure Resource Graph, Log Analytics, or incident-management integrations.

## End-To-End Example

If the user asks, "What active alerts do we have?":

1. `Program.cs` reads the console input.
2. `OperationsAgent` adds the enterprise-operations system prompt.
3. `AiRequestOrchestrator` selects the configured provider.
4. The provider creates a Semantic Kernel with its model connector.
5. `AiKernelFactory` registers `AzureAlertPlugin`.
6. Semantic Kernel sends the chat request with available functions.
7. The model may call `azure_alerts.GetActiveAlertsAsync`.
8. `AzureAlertPlugin` delegates to `MockAlertService`.
9. The provider returns the final assistant response.
10. `Program.cs` prints the response.

## Current Limitations

- Alert data is mocked; no live Azure APIs are called.
- A new kernel is created per request, so no chat history is preserved between
  turns.
- Provider selection is string-based even though `AiProviderType` exists.
- Gemini and Ollama may differ in tool-calling behavior depending on model and
  connector support.
- Several model and interface files currently use the global namespace, while
  most implementation files use `EnterpriseAiPortfolio.*` namespaces.

## Extension Points

- Add a real `AzureMonitorAlertService` implementing `IAlertService`.
- Add more plugins for logs, metrics, resource health, change history, or runbook
  lookup.
- Add conversation memory by storing and passing chat history across turns.
- Add another provider by implementing `IAiProvider`, registering it in DI, and
  extending `AiProviderFactory`.
- Move provider configuration to environment variables or user secrets for
  sensitive values such as Gemini API keys.
