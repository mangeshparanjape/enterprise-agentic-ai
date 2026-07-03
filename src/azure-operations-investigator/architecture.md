# Azure Operations Investigator

## Architeture flow

Program.cs
 → OperationsAgent
 → AiRequestOrchestrator
 → AiProviderFactory
 → OllamaProvider / GeminiProvider
 → Semantic Kernel
 → AzureAlertPlugin
