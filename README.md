# CPP.Framework.Libraries

An enterprise utility framework targeting .NET Framework 4.8 (C# 7.3). It provides foundational services for dependency injection, configuration, security, cryptography, data access, serialization, web/REST infrastructure, and Azure cloud integration.

## Solution Structure

The solution contains six production libraries and a unit test project.

```
CPP.Framework.Libraries.sln
  CPP.Framework.Core                              Core services and utilities
  CPP.Framework.EntityData                        Entity Framework 6 helpers
  CPP.Framework.Messaging                         Email transport (SMTP / SendGrid)
  CPP.Framework.Serialization                     JSON, Protocol Buffers, compression
  CPP.Framework.Web                               REST clients, MVC / Web API security
  CPP.Framework.WindowsAzure                      Azure Storage, Service Bus, WebJobs
  CPP.Framework.WindowsAzure.ApplicationInsights  Application Insights diagnostics
  CPP.Framework.UnitTests                         Unit test suite
```

### Project Dependencies

```
Core  <──  EntityData
      <──  Messaging
      <──  Serialization
      <──  Web
      <──  WindowsAzure  <──  WindowsAzure.ApplicationInsights
```

## Projects

### CPP.Framework.Core

Foundational services and utilities consumed by every other project.

- **Dependency Injection** — Unity-based `ServiceLocator` with XML and code-first registration, custom constructor selection, and thread-safe resolution. `CodeServiceSingleton` and `CodeServiceProvider` manage singleton lifecycles with startup/cleanup hooks.
- **Configuration** — `ConfigSettingProvider` bridges environment variables, `app.config`/`web.config`, and default values through the strongly-typed `ConfigSettingKey` enum.
- **Diagnostics** — Journaling and telemetry infrastructure with pluggable listeners.
- **Cryptography** — `CryptographyService` singleton with certificate-based encryption (`AesCryptoBundle`, `AsymmetricCryptoBundle`) and pluggable certificate providers (current user, machine store, file).
- **Security** — Claims-based authorization via `SecurityAuthorizationManager`, with policy groups (`SecurityAccessRightPolicy`, `SecurityAccessRolesPolicy`) and claim-type comparers.
- **Validation** — `ArgumentValidator` (expression-based parameter validation) and `ObjectValidator` (data annotations plus custom rules). Custom attributes include `RequiredIfAttribute`, `DependentValidationAttribute`, and `UniqueAttribute`.
- **Collections** — `SetDictionary`, `StateChangeMap`, `ContraVariantCollection`, `CompoundDictionary`.
- **Messaging** — Mail message abstractions (`IMailMessage`) and attachment providers for file, web, and Azure blob sources.
- **Threading** — `MultiAccessLock`, `AsyncLock`, `ThreadLimiter`, `CancellableResourceManager`.
- **ComponentModel** — `DynamicPropertyAccessor`, `ObservableModel`.

### CPP.Framework.EntityData

Helpers for Entity Framework 6 database-first workflows.

- `EFXDataSourceFilter` — Extends `DataSourceFilter` with EF query integration.
- Typed exceptions for missing entities, invalid relations, and general data entity errors.
- Entity relation helpers and journal source extensions for EF operations.

### CPP.Framework.Messaging

Email transport layer.

- `MailTransportProvider` abstract base with `SmtpTransportProvider` and `SendGridTransportProvider` implementations.
- Active Directory recipient resolution via `System.DirectoryServices.AccountManagement`.

### CPP.Framework.Serialization

Serialization and compression utilities.

- **JSON** — `JsonKnownTypeConverter` for polymorphic deserialization, `EncryptingJsonConverter` and `ConfidentialContractResolver` for encrypting sensitive properties, `UtcDateTimeConverter` for consistent UTC handling. All built on Newtonsoft.Json.
- **Protocol Buffers** — `ProtocolSerializer` wrapper around protobuf-net with `AssemblyKnownTypeAttribute` for type registration.
- **Compression** — `GzipProtocolSerializer` for compressed binary transport.

### CPP.Framework.Web

MVC / Web API and REST client infrastructure.

- `HttpServiceClient` — Abstract strongly-typed REST client with URI validation, HTTP/HTTPS support, and pluggable message handlers.
- Parameter formatters for API, HTTP, and WCF query string styles.
- `SecurityAuthorizeAttribute` and `SecurityAllowAnonymousAttribute` for controller-level authorization.
- `HttpServiceRequestFailedException<TCode>` for typed error codes.

### CPP.Framework.WindowsAzure

Azure cloud service abstractions.

- **Service Bus** — `AzureServiceBus` manager with `AzureServiceBusTopic`, `AzureServiceBusQueue`, and `AzureServiceBusSubscription`. `AzureMessagePropertyAttribute` maps message properties declaratively.
- **Storage** — `AzureStorageBlob`, `AzureStorageBlockBlob`, `AzureStorageTable`, `AzureStorageQueue` wrappers with retry policies and `AzureStorageLeaseManager`. `AzureTableEntity` and `AzureTableEntitySerializer` for Table Storage.
- **WebJobs** — Host integration with Service Bus triggers and journal listener support.
- **Configuration** — `CloudConfigProvider` and `RoleEnvironmentService` for Azure Cloud Services role configuration.
- **Diagnostics** — `AzureStorageJournalListener` and `AzureWebJobJournalListener` for telemetry routing.
- **Messaging** — `AzureAttachmentProvider` for blob-backed mail attachments.

### CPP.Framework.WindowsAzure.ApplicationInsights

Application Insights integration.

- Journal listener that routes framework events to Application Insights as traces and exceptions.
- Configurable `TelemetryClient` wrapper and configuration extensions.

### CPP.Framework.UnitTests

Unit test suite covering all six core libraries. Uses MSTest v3 as the test framework with NSubstitute for mocking and FluentAssertions for assertions (migrated from Rhino Mocks and custom `Verify` helpers).

Test baseline: 474 tests total, 455 passing, 19 skipped (pre-existing `[Ignore]`).

## Key Architectural Patterns

| Pattern | Implementation |
|---------|---------------|
| Dependency Injection | Unity 4.0 container behind a static `ServiceLocator` singleton |
| Service Lifecycle | `CodeServiceSingleton` with startup/cleanup hooks via `CodeServiceProvider` |
| Configuration | Multi-source bridging: environment variables, app.config, Azure role config, defaults |
| Authorization | Claims-based policies evaluated by `SecurityAuthorizationManager` |
| Validation | Composite: data annotations + expression-based `ArgumentValidator` + `ICustomArgumentValidator<T>` |
| Serialization | Pluggable JSON (Newtonsoft) and Protocol Buffer (protobuf-net) pipelines with encryption and compression |
| REST Clients | Abstract `HttpServiceClient` with pluggable parameter formatters and message handlers |
| Thread Safety | `MultiAccessLock` (reader/writer), `AsyncLock`, `Lazy<T>` throughout |

## External Dependencies

| Package | Version | Used By |
|---------|---------|---------|
| Unity | 4.0.1 | Core |
| CommonServiceLocator | 1.3.0 | Core |
| Microsoft.Extensions.Configuration | 5.0.0 | Core |
| EntityFramework | 6.4.4 | EntityData |
| Newtonsoft.Json | 13.0.3 | Serialization, Web, WindowsAzure |
| protobuf-net | 2.0.0.668 | Serialization |
| SendGrid | 1.0.2 | Messaging |
| Microsoft.AspNet.Mvc | 5.2.9 | Web |
| Microsoft.AspNet.WebApi | 5.2.9 | Web |
| Microsoft.AspNet.WebApi.Versioning | 2.3.0 | Web |
| WindowsAzure.Storage | 4.3.0 | WindowsAzure |
| WindowsAzure.ServiceBus | 2.7.6 | WindowsAzure |
| Microsoft.Azure.WebJobs | 1.1.1 | WindowsAzure |
| MSTest.TestFramework | 3.6.3 | UnitTests |
| NSubstitute | 5.x | UnitTests |
| FluentAssertions | 7.1 | UnitTests |

## Building

Open `CPP.Framework.Libraries.sln` in Visual Studio 2022+ and build. All projects target `net48`.

## Running Tests

Tests target .NET Framework 4.8 and require `vstest.console.exe`:

```
"C:\Program Files\Microsoft Visual Studio\<version>\<edition>\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" CPP.Framework.UnitTests\bin\Debug\CPP.Framework.UnitTests.dll
```
