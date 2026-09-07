name: dotnet-azure-function-starter description: Build and maintain the organization’s reusable .NET 10 C# Azure Functions starter template. Use this skill when creating, reviewing, or modifying the generic Azure Function App + xUnit starter solution, its coding standards, build standards, documentation baseline, or template packaging.

.NET Azure Function Starter Template

Use this skill when working on the reusable enterprise starter template for C# Azure Functions.

Goal

Maintain a small, generic, reusable starter template that gives developers a consistent baseline without imposing business-specific architecture or unnecessary frameworks.

The starter must contain:

• One .NET 10 Azure Functions project using the isolated worker model.
• One xUnit test project.
• Basic C# coding-standard enforcement.
• Common build settings.
• Central package-version management.
• Basic DocFX support.
• A simple sample Function and sample test that demonstrate the expected development pattern.

Do not add domain-specific or payment-specific business logic to the starter template.

Standard Solution Structure

Use this physical folder convention:

```text
src/
└── api/
    └── {domain}/
        └── {capability}/
            ├── function/
            │   ├── FunctionApp.csproj
            │   ├── Program.cs
            │   ├── host.json
            │   └── Functions/
            │
            └── function.tests/
                ├── FunctionApp.Tests.csproj
                └── Tests/
```

Example:

```text
src/api/payment/search/function
src/api/payment/search/function.tests
```

The template itself must remain generic. {domain} and {capability} are placeholders supplied when developers instantiate or customize the template.

Naming

Keep physical project folders short.

Preferred folder names:

```text
function
function.tests
```

The organization controls assembly naming through MSBuild.

Use:

```xml
<AssemblyName>gov.company.$(MSBuildProjectName)</AssemblyName>
```

Do not embed business-specific names into the base template.

Use PascalCase for project names and namespaces even when physical folders use lowercase.

Runtime

Use:

```xml
<TargetFramework>net10.0</TargetFramework>
<AzureFunctionsVersion>v4</AzureFunctionsVersion>
```

Use the Azure Functions isolated worker model only.

Do not create an in-process Functions project.

Projects

Keep the starter to exactly two projects unless explicitly requested otherwise:

1. Azure Function App project.
2. xUnit test project.

Do not automatically add:

• Domain projects.
• Application projects.
• Infrastructure projects.
• Repository projects.
• Shared-kernel projects.
• MediatR.
• AutoMapper.
• FluentValidation.
• Database libraries.
• Messaging libraries.
• Business-specific NuGet packages.

Additional architecture belongs to individual applications, not the generic starter.

Root Standards Files

The starter should support these files at the repository or solution root:

```text
.editorconfig
Directory.Build.props
Directory.Packages.props
global.json
docfx.json
README.md
```

global.json may be omitted if the organization deliberately allows any supported .NET 10 SDK. If present, use it to pin the approved SDK/toolchain version.

.editorconfig Responsibilities

Use .editorconfig for C# source-code standards.

Baseline rules should cover:

• Four-space indentation.
• UTF-8.
• Final newline.
• No trailing whitespace.
• Braces required.
• File-scoped namespaces.
• Naming conventions.
• Explicit accessibility modifiers.
• Consistent var usage.
• using placement and ordering.
• Analyzer severity where appropriate.

Prefer strong enforcement for deterministic conventions and moderate enforcement for subjective style rules.

Do not use .editorconfig to enforce:

• Test coverage percentages.
• DocFX execution.
• SDK version.
• NuGet versions.
• CI/CD behavior.
• Business architecture.

Namespace Standard

Use file-scoped namespaces.

Example:

```csharp
namespace Company.FunctionApp.Functions;
```

Namespaces must use PascalCase.

Avoid forcing every namespace to mirror every physical folder segment unless the organization explicitly chooses that convention.

For generated application code, namespaces should reflect the logical project/component rather than blindly including src, api, or other repository-organization folders.

Directory.Build.props Responsibilities

Use Directory.Build.props for common MSBuild/compiler settings inherited by both projects.

Recommended baseline:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <AssemblyName>gov.company.$(MSBuildProjectName)</AssemblyName>
  </PropertyGroup>
</Project>
```

Do not automatically enable TreatWarningsAsErrors globally unless explicitly approved. Prefer promoting selected important analyzers to errors.

Directory.Packages.props Responsibilities

Use central package management for shared package versions.

Centralize only baseline dependencies used by the starter, such as:

• Microsoft.Azure.Functions.Worker
• Microsoft.Azure.Functions.Worker.Sdk
• HTTP trigger extension packages when needed
• Application Insights packages
• xUnit
• Microsoft.NET.Test.Sdk
• Approved test-support packages

Do not add packages solely because they might be useful later.

Projects should reference centrally managed packages without repeating versions.

global.json Responsibilities

If used, global.json controls the .NET SDK/toolchain selected by dotnet commands.

It does not replace:

```xml
<TargetFramework>net10.0</TargetFramework>
```

The target framework defines what the application targets. global.json defines which SDK builds it.

Keep the SDK major version aligned with the target framework unless there is an explicitly approved reason not to.

Azure Function Standard

Keep Function trigger classes thin.

A Function should primarily:

1. Receive the trigger/input.
2. Perform boundary-level parsing or validation.
3. Call normal C# services when business logic is needed.
4. Return the appropriate response.
5. Use dependency injection.
6. Use structured logging.
7. Propagate CancellationToken for asynchronous I/O.

Do not put large amounts of business logic directly in the trigger method.

Use constructor injection instead of service locators or manually creating infrastructure dependencies.

Async Standard

Use async/await for I/O-bound work.

Do not make purely synchronous code asynchronous without a reason.

Async methods should generally use the Async suffix.

Propagate CancellationToken through downstream asynchronous operations.

Logging Standard

Use:

```csharp
ILogger<T>
```

Use structured logging:

```csharp
_logger.LogInformation(
    "Processing request {RequestId}",
    requestId);
```

Do not prefer string interpolation for structured log messages.

Never include secrets, credentials, access tokens, connection strings, or sensitive business data in logs.

The base starter should demonstrate the logging pattern but must not define business-specific error catalogs or troubleshooting logic.

Testing Standard

Use xUnit.

The test project should reference the Function App project.

Prefer testing normal C# classes directly rather than Azure Functions runtime implementation details.

Use clear behavioral test names, for example:

```csharp
Method_WhenCondition_ExpectedResult
```

Use Arrange / Act / Assert where it improves readability.

Include one simple sample test in the starter.

Do not force a mocking library unless the organization has explicitly standardized one.

DocFX Baseline

DocFX is part of the starter documentation baseline.

Include:

```text
docfx.json
docs/
```

Enable XML documentation generation:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

This allows C# XML comments to be included in generated API documentation.

DocFX itself is not enforced through .editorconfig.

The CI/CD pipeline should eventually validate that DocFX builds successfully.

For the basic starter, developers should document each Azure Function at a useful level.

Function documentation may include:

• Purpose.
• Trigger or route.
• Inputs.
• Outputs.
• Dependencies.
• Related xUnit tests.
• Runbook reference when one is required.

Do not require a runbook for every Function.

Keep XML comments concise and use DocFX Markdown pages for richer operational documentation.

Sample Code

The starter may contain one simple sample HTTP-trigger Function.

The sample exists only to demonstrate:

• Function attributes.
• Constructor injection.
• ILogger<T>.
• Async/cancellation patterns when appropriate.
• Namespace and naming standards.
• Test organization.

Do not include real business workflows, real endpoints, production identifiers, secrets, or environment-specific configuration.

Configuration

Keep host.json generic.

Never commit real secrets to the starter.

If local.settings.json is used for local examples:

• Use placeholders only.
• Ensure real local settings are excluded from source control.
• Do not include real connection strings, tokens, certificates, passwords, or keys.

README

The starter README should explain:

• Purpose of the starter.
• Required .NET version.
• How to restore.
• How to build.
• How to run the Function App locally.
• How to run tests.
• How to build DocFX documentation.
• Which files developers should modify when creating a real application.

Keep README instructions generic.

Template Conversion

The starter can be created from an existing working Function App solution.

Before packaging it as a reusable dotnet new template:

1. Remove all business-specific logic.
2. Remove business-specific models.
3. Remove real integrations.
4. Remove environment-specific settings.
5. Remove secrets and credentials.
6. Keep only generic infrastructure and development conventions.
7. Add the standard root files.
8. Verify build and tests.
9. Verify DocFX.
10. Add .template.config/template.json.

Use the .NET template engine to replace appropriate template names when a developer creates a project.

Do not hard-code a specific business domain into the reusable template.

Review Checklist

When reviewing changes to the starter, verify:

☐ The Function project targets .NET 10 isolated.
☐ There are exactly two baseline projects: Function App and xUnit tests.
☐ No business-specific logic has entered the starter.
☐ .editorconfig contains only generally applicable coding rules.
☐ Common compiler/build settings are centralized in Directory.Build.props.
☐ Package versions are centrally managed where appropriate.
☐ Nullable reference types are enabled.
☐ .NET analyzers are enabled.
☐ XML documentation generation is enabled.
☐ DocFX configuration remains generic.
☐ Function sample code is thin.
☐ Dependency injection is used correctly.
☐ Logging is structured.
☐ Tests use xUnit.
☐ No secrets or environment-specific configuration are committed.
☐ Folder convention remains src/api/{domain}/{capability}/function and function.tests.
☐ The generated solution builds and tests successfully.

Decision Principle

When deciding whether something belongs in the starter, use this rule:

> If every Azure Function application should receive it regardless of business domain, it can belong in the starter. If it depends on a specific application’s architecture, integration, database, workflow, or business rules, leave it out.