# Azure Operations Investigator

## Architeture flow

Program.cs
 → OperationsAgent
 → AiRequestOrchestrator
 → AiProviderFactory
 → OllamaProvider / GeminiProvider
 → Semantic Kernel
 → AzureAlertPlugin

## Sequence

Console
   │
   ▼
OperationsAgent
   │
   ▼
AiRequestOrchestrator
   │
   ▼
IAiProviderFactory
   │
   ▼
IAiProvider
   │
   ▼
IAiKernelFactory
   │
   ▼
Semantic Kernel
   │
   ▼
Plugins
   │
   ▼
Business Services
