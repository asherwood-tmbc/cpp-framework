# Unit Test Migration Plan

**Scope:** `CPP.Framework.UnitTests`
**Goal:** Remove the dependency on `CPP.Framework.Testing` and `RhinoMocks`; replace with MSTest v3 (already referenced), NSubstitute, and FluentAssertions v7.1.

---

## 1. Package Changes

### CPP.Framework.UnitTests.csproj

| Action | Package |
|--------|---------|
| Remove | `RhinoMocks` v3.6.1 |
| Remove | Project reference → `CPP.Framework.Testing` |
| Add | `NSubstitute` (latest 5.x for net48) |
| Add | `FluentAssertions` v7.1 |
| Keep | `MSTest.TestAdapter` v3.6.3 |
| Keep | `MSTest.TestFramework` v3.6.3 |

`CPP.Framework.Testing.csproj` itself is left alone (it is referenced by other projects). Only the **UnitTests project reference** to it is removed.

---

## 2. Quick-Reference Replacement Mapping

### Assertions

| Current (`Verify.*` / custom) | Replacement (FluentAssertions) |
|-------------------------------|-------------------------------|
| `Verify.IsTrue(x)` | `x.Should().BeTrue()` |
| `Verify.IsFalse(x)` | `x.Should().BeFalse()` |
| `Verify.IsNull(x)` | `x.Should().BeNull()` |
| `Verify.IsNotNull(x)` | `x.Should().NotBeNull()` |
| `Verify.AreEqual(expected, actual)` | `actual.Should().Be(expected)` |
| `Verify.AreNotEqual(a, b)` | `actual.Should().NotBe(expected)` |
| `Verify.AreSame(a, b)` | `actual.Should().BeSameAs(expected)` |
| `Verify.AreNotSame(a, b)` | `actual.Should().NotBeSameAs(expected)` |
| `Verify.IsInstanceOfType(obj, typeof(T))` | `obj.Should().BeOfType<T>()` or `.BeAssignableTo<T>()` |
| `Verify.IsNotInstanceOfType(obj, typeof(T))` | `obj.Should().NotBeOfType<T>()` |
| `Verify.Fail(msg)` | `Assert.Fail(msg)` (keep MSTest directly — FA has no equivalent static) |
| `Verify.AreEqual(float, float, delta)` | `actual.Should().BeApproximately(expected, delta)` |
| `Verify.AreEqual(string, string, ignoreCase)` | `actual.Should().BeEquivalentTo(expected)` (case-insensitive) or `.Be(expected)` |
| `Verify.IsSubsetOf(actual, expected)` (IDictionary) | `actual.Should().ContainKeys(expected.Keys)` + per-value `.Should().Be()` |
| `Verify.AreEqual(HashSet, HashSet)` | `actual.Should().BeEquivalentTo(expected)` |
| `Verify.AreEqual(List, List)` | `actual.Should().Equal(expected)` (ordered) |
| `Verify.AreEqual(List, List, comparer)` | `actual.Should().Equal(expected, (a, b) => comparer(a, b))` |
| `Assert.IsTrue/IsFalse/...` (already direct, in 2 files) | Same FluentAssertions conversions above |

### Expected-Exception Attributes → Inline Assertions

All `[ExpectedArgumentException]`, `[ExpectedArgumentNullException]`, and `[ExpectedArgumentOutOfRangeException]` attributes are **removed from the method signature** and the test body is restructured to use FluentAssertions:

```csharp
// BEFORE
[TestMethod]
[ExpectedArgumentNullException("argument")]
public void MyTest()
{
    SomeMethod(null);
}

// AFTER
[TestMethod]
public void MyTest()
{
    var act = () => SomeMethod(null);
    act.Should().Throw<ArgumentNullException>()
       .WithParameterName("argument");
}
```

For `[ExpectedArgumentOutOfRangeException("paramName")]`:
```csharp
act.Should().Throw<ArgumentOutOfRangeException>()
   .WithParameterName("paramName");
```

For plain `[ExpectedException(typeof(SomeException))]` (already MSTest-native, used in a few tests):
These can stay as-is or optionally be converted to:
```csharp
act.Should().Throw<SomeException>();
```
Converting them is recommended for consistency and to get better failure messages.

### Test Categorization

`[TestGroup(TestGroupTarget.Core)]` → `[TestCategory("Core")]`
`[TestGroup(TestGroupTarget.DependencyInjection)]` → `[TestCategory("DependencyInjection")]`

Full mapping of `TestGroupTarget` enum → category strings:

| `TestGroupTarget` | `[TestCategory(...)]` string |
|-------------------|------------------------------|
| `Configuration` | `"Configuration"` |
| `Core` | `"Core"` |
| `DependencyInjection` | `"DependencyInjection"` |
| `Validation` | `"Validation"` |
| `Extentions` | `"Extentions"` *(keep the existing typo to avoid breaking filter scripts)* |

Alternatively, `TestGroupAttribute` and `TestGroupTarget` are simple and can be **copied directly into the UnitTests project** under a suitable namespace, keeping the attribute usable without any test changes. This is the lower-risk option.

### Mocking

| Current (RhinoMocks via StubFactory) | Replacement (NSubstitute) |
|--------------------------------------|--------------------------|
| `StubFactory.CreateStub<T>()` | `Substitute.For<T>()` |
| `StubFactory.CreatePartial<T>()` | `Substitute.ForPartsOf<T>()` |
| `service.StubAction(x => x.Prop).Return(v)` | `service.Prop.Returns(v)` |
| `service.StubAction(x => x.Method(arg)).Return(v)` | `service.Method(arg).Returns(v)` |
| `service.StubAction(x => x.Method(Arg<T>.Is.Equal(v))).Return(r)` | `service.Method(v).Returns(r)` |
| `service.StubAction(x => x.Method(Arg<T>.Is.Anything)).Return(v)` | `service.Method(Arg.Any<T>()).Returns(v)` |
| `service.StubAction(x => x.VoidMethod(...)).DoNothing()` | *(no setup needed — NSubstitute void methods are no-ops by default)* |
| `service.StubAction(x => x.Method(...)).Throw<TEx>()` | `service.Method(...).Throws<TEx>()` |
| `service.StubAction(x => x.Method(...)).Throw(exInstance)` | `service.Method(...).Throws(exInstance)` |
| `service.StubAction(x => x.VoidMethod(...)).Throw<TEx>()` | `service.When(x => x.VoidMethod(...)).Throw<TEx>()` |
| `service.StubAction(x => x.VoidMethod()).WhenCalled(mi => {...})` | `service.When(x => x.VoidMethod()).Do(ci => {...})` |
| `service.StubAction(x => x.Method()).WhenCalled(mi => {...}).Return(v)` | `service.Method().Returns(ci => { /*callback*/ return v; })` |
| `service.StubAction(...).CallOriginalMethod()` | *(use `Substitute.ForPartsOf<T>()` — base is called unless overridden)* |
| `StubFactory.CreateInstance<T>(args)` | `Activator.CreateInstance(typeof(T), BindingFlags.NonPublic\|BindingFlags.Instance, null, args, null)` — extract to a static helper in UnitTests |
| `Arg<T>.Is.Equal(v)` | `v` directly, or `Arg.Is<T>(x => x == v)` |
| `Arg<T>.Is.Anything` | `Arg.Any<T>()` |
| `Arg.Is(value)` (non-generic) | `Arg.Is<T>(value)` |

### ServiceLocator Registration

`StubFactory.RegisterServiceStub<T>(stub)` calls `ServiceLocator.Register<T>(stub)`. This logic (including the `StubServiceRegistrationAttribute` path) should be **extracted into a local static helper** inside UnitTests (e.g., `TestServiceLocator` or `ServiceStubHelper`). The helper is identical except it no longer depends on RhinoMocks internals.

`ServiceLocator.Unload()` in `[TestCleanup]` methods is **unchanged** — it is a framework concept, not a testing-library concept.

---

## 3. Components: Keep, Move, or Drop

### From CPP.Framework.Testing — what happens to each piece

| Component | Action |
|-----------|--------|
| `TestSuite` base class | **Drop** — no test class in UnitTests inherits it anyway |
| `TestGroupAttribute` + `TestGroupTarget` | **Option A:** Copy into UnitTests (zero test changes). **Option B:** Replace with inline `[TestCategory]` strings. |
| `ExpectedArgumentExceptionAttribute` hierarchy | **Drop** — replace with inline FluentAssertions (see §2) |
| `Verify` class (Dynamic, Collections, IDictionary) | **Drop** — replace with FluentAssertions |
| `StubFactory.CreateStub/CreatePartial` | **Drop** — replaced by `Substitute.For/ForPartsOf` |
| `StubFactory.RegisterServiceStub/RegisterInterfaceStub/RegisterPrincipal` | **Move** — extract DI-registration logic to a local `ServiceStubHelper` static class in UnitTests; strip all RhinoMocks dependencies |
| `StubFactory.CreatePrincipal/GrantAccessRight/GrantFeatureName/etc.` | **Move** — copy extension methods to a local `ClaimsPrincipalTestExtensions` class; these have no RhinoMocks dependency |
| `StubFactory.CreateInstance<T>()` | **Move** — copy as a static helper method; replace reflection call with `Activator.CreateInstance` using `BindingFlags.NonPublic` |
| `StubFactory.CreateDependentStubs/StubAction` | **Drop** — replaced by direct NSubstitute API |
| `StubActionContext<T>` / `StubActionContext<T,R>` | **Drop** — replaced by NSubstitute fluent API |
| `ConfigSettingsStubExtensions` | **Rewrite** — see §4.4 |
| `StubServiceRegistrationAttribute` | **Move** — copy the attribute class into UnitTests; it has no testing-library dependency |
| `MethodInvocationExtensions` (Rhino.Mocks namespace) | **Drop** — RhinoMocks-specific |
| `ObjectEqualityComparer<T>` | **Drop or keep as convenience** — FluentAssertions `BeEquivalentTo()` covers its use cases; however the class has no RhinoMocks dependency and can be copied if still wanted |
| `TestMemoryStream` | **Move** — copy into UnitTests; no testing-library dependency |
| `ExtendedContext` (in UnitTests project itself) | **No change** — already in the UnitTests project |
| `ErrorStrings` (internal) | **Drop** — only used by the expected-exception attributes |

---

## 4. Detailed Migration Steps

### 4.1 Package Setup

1. Open `CPP.Framework.UnitTests.csproj`.
2. Remove `<PackageReference Include="RhinoMocks" />`.
3. Remove `<ProjectReference Include="..\CPP.Framework.Testing\CPP.Framework.Testing.csproj" />`.
4. Add:
   ```xml
   <PackageReference Include="NSubstitute" Version="5.*" />
   <PackageReference Include="FluentAssertions" Version="7.1.*" />
   ```

### 4.2 Create Local Helper Infrastructure

Create a folder `Testing/` inside UnitTests and add the following files. These are simple extractions of non-framework-specific code:

**`Testing/ServiceStubHelper.cs`**
Replaces `StubFactory.RegisterServiceStub`, `RegisterInterfaceStub`, and `RegisterPrincipal`:
```csharp
internal static class ServiceStubHelper
{
    public static T RegisterServiceStub<T>(T stub, string registrationName = null)
    {
        var attr = typeof(T).GetCustomAttributes<StubServiceRegistrationAttribute>().ToList();
        if (attr.Any())
        {
            foreach (var reg in attr)
                ServiceLocator.Register(reg.InterfaceType, stub, reg.RegistrationName);
        }
        else if (stub is IPrincipal)
            ServiceLocator.Register<IPrincipal>(stub as IPrincipal);
        else if (registrationName != null)
            ServiceLocator.Register<T>(stub, registrationName);
        else
            ServiceLocator.Register<T>(stub);
        return stub;
    }

    public static void RegisterInterfaceStub<TInterface, TProvider>()
        => ServiceLocator.Register<TInterface, TProvider>();
}
```

**`Testing/ReflectionHelper.cs`**
Replaces `StubFactory.CreateInstance<T>()`:
```csharp
internal static class ReflectionHelper
{
    public static T CreateInstance<T>(params object[] args)
        => (T)Activator.CreateInstance(
               typeof(T),
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
               null, args, null);
}
```

**`Testing/ClaimsPrincipalTestExtensions.cs`**
Move the principal/claims helper extension methods from `StubFactory` verbatim (they have no RhinoMocks dependency):
```csharp
internal static class ClaimsPrincipalTestExtensions
{
    public static ClaimsPrincipal CreatePrincipal(string authenticationType) { ... }
    public static ClaimsPrincipal GrantAccessRight(this ClaimsPrincipal principal, string accessRight) { ... }
    public static ClaimsPrincipal GrantFeatureName(this ClaimsPrincipal principal, string featureName) { ... }
    public static ClaimsPrincipal GrantClaim(this ClaimsPrincipal principal, ...) { ... }
    public static ClaimsPrincipal GrantUserName(this ClaimsPrincipal principal, string userName) { ... }
    // etc.
}
```

**`Testing/StubServiceRegistrationAttribute.cs`**
Copy the attribute class verbatim from CPP.Framework.Testing.

**`Testing/TestGroupAttribute.cs`** + **`Testing/TestGroupTarget.cs`** *(if Option A is chosen)*
Copy verbatim.

**`Testing/TestMemoryStream.cs`**
Copy verbatim.

### 4.3 Migrate `StubDefaultActions.cs`

This file already lives in UnitTests. Rewrite using NSubstitute:

```csharp
// ConfigurationManagerService
internal static ConfigurationManagerService RegisterServiceStub(
    this ConfigurationManagerService service)
{
    service.AppSettings.Returns(new NameValueCollection());
    service.ConnectionStrings.Returns(new ConnectionStringSettingsCollection());
    service.GetConfigurationSettingValue(Arg.Any<string>()).Throws<ConfigurationErrorsException>();
    // ... other methods
    return ServiceStubHelper.RegisterServiceStub(service);
}

// FileService
internal static FileService RegisterServiceStub(this FileService service)
{
    service.Exists(Arg.Any<string>()).Returns(false);
    return ServiceStubHelper.RegisterServiceStub(service);
}

// RoleEnvironmentService — note: needs non-virtual member review
internal static RoleEnvironmentService RegisterServiceStub(
    this RoleEnvironmentService service)
{
    service.IsAvailable.Returns(false);
    service.IsAzureStorageEmulatorActive.Returns(false);
    service.GetConfigurationSettingValue(Arg.Any<string>()).Throws<InvalidOperationException>();
    // For RoleEnvironmentException use ReflectionHelper.CreateInstance<RoleEnvironmentException>()
    return ServiceStubHelper.RegisterServiceStub(service);
}
```

> **Note:** If any stubbed method is not `virtual` (common with concrete service classes in .NET Framework), NSubstitute will throw `CouldNotSetReturnDueToNoInterceptionException` at setup time. In that case the service must either be extracted to an interface or a hand-written fake/wrapper class used instead. Audit each service class for virtual members before completing this step.

### 4.4 Rewrite `ConfigSettingsStubExtensions`

The current implementation creates a RhinoMocks `NameValueCollection` partial stub and stubs `Get(name)` on it. Because `NameValueCollection.Get(string)` is **not virtual** in .NET Framework, this relied on RhinoMocks proxying the partial class (which may work via `DynamicProxy` interceptors for certain method call patterns). With NSubstitute, the clean equivalent is to use a **real `NameValueCollection`** instance:

```csharp
// BEFORE (RhinoMocks)
service.StubConfigSetting("Foo", "Bar");
// → stubs AppSettings.Get("Foo") to return "Bar" via RhinoMocks proxy

// AFTER (NSubstitute + real collection)
var appSettings = new NameValueCollection { { "Foo", "Bar" } };
service.AppSettings.Returns(appSettings);
```

For the throw-on-missing-key scenario, stub at the `ConfigurationManagerService` method level (which is virtual) rather than at the `NameValueCollection.Get` level:

```csharp
service.GetConfigurationSettingValue("MissingKey")
       .Throws(new ConfigurationErrorsException("..."));
```

Rewrite `ConfigSettingsStubExtensions` accordingly:

```csharp
internal static ConfigurationManagerService StubConfigSetting(
    this ConfigurationManagerService service, string name, string value)
{
    // Accumulate into the real collection returned by AppSettings
    var collection = service.AppSettings ?? new NameValueCollection();
    collection.Add(name, value);
    service.AppSettings.Returns(collection);
    return service;
}

internal static ConfigurationManagerService StubConfigSetting(
    this ConfigurationManagerService service, string name, Func<Exception> factory)
{
    service.GetConfigurationSettingValue(name).Throws(factory());
    return service;
}

internal static ConfigurationManagerService StubConnectionString(
    this ConfigurationManagerService service,
    string name, string connectionString, string providerName = "")
{
    var existing = service.ConnectionStrings ?? new ConnectionStringSettingsCollection();
    existing.Add(new ConnectionStringSettings(name, connectionString, providerName));
    service.ConnectionStrings.Returns(existing);
    return service;
}
```

`ConfigSettingKey` overloads simply call `configKey.GetConfigSettingName()` and delegate — these are unchanged in pattern.

### 4.5 Migrate Test Files (File by File)

#### `ArgumentValidatorTests.cs`
- **Assertions:** Replace all `Verify.AreEqual(...)` with `actual.Should().Be(expected)`.
- **Expected-exception attributes:** Every test decorated with `[ExpectedArgumentException("argument")]` or `[ExpectedArgumentNullException("argument")]` must be refactored:
  ```csharp
  // BEFORE
  [TestMethod, ExpectedArgumentNullException("argument")]
  public void ValidateNotNullWithNull() { ArgumentValidator.ValidateNotNull(null, "argument"); }

  // AFTER
  [TestMethod]
  public void ValidateNotNullWithNull()
  {
      var act = () => ArgumentValidator.ValidateNotNull(null, "argument");
      act.Should().Throw<ArgumentNullException>().WithParameterName("argument");
  }
  ```
- **`[TestGroup]` attributes:** Replace per §2 (or copy attribute, zero change).
- The `ThrowArgumentExceptionFor*` helper methods that currently re-throw after inspecting exception properties should be rewritten to use `Should().Throw<T>().And.Message.Should().Contain(...)`.

#### `ConfigSettingKeyExtensionsTests.cs`
- Replace `Verify.AreEqual` → `.Should().Be()`.
- Replace `Verify.IsNull` → `.Should().BeNull()`.
- Replace `[ExpectedArgumentException("configKey")]` attributes with inline FluentAssertions throws.

#### `ConfigSettingProviderTests.cs`
- Replace `StubFactory.CreateStub<ConfigurationManagerService>()` → `Substitute.For<ConfigurationManagerService>()`.
- Replace `.StubConfigSetting(...)` calls → rewritten extension method (§4.4).
- Replace `.RegisterServiceStub()` → `ServiceStubHelper.RegisterServiceStub(stub)`.
- Replace `Verify.*` → FluentAssertions.
- `[ExpectedException(typeof(ConfigurationErrorsException))]` → inline `act.Should().Throw<ConfigurationErrorsException>()`.

#### `RoleConfigProviderTests.cs`
- Same substitution pattern as `ConfigSettingProviderTests`.
- `StubFactory.CreateInstance<RoleEnvironmentException>()` → `ReflectionHelper.CreateInstance<RoleEnvironmentException>()`.
- All `Verify.*` → FluentAssertions.
- `[ExpectedException(...)]` attributes → inline throws.

#### `ServiceLocatorTests.cs`
- Replace all `Verify.*` → FluentAssertions.
- Remove `using CPP.Framework.Diagnostics.Testing;` (for `TestGroupTarget`); add `using` for `[TestCategory]` or local `TestGroupTarget`.
- Replace `[TestGroup(...)]` attributes per §2.
- `[ExpectedArgumentNullException]` / `[ExpectedArgumentException]` attribute uses on ~15 methods → inline FluentAssertions throws.
- `StubFactory.CreateInstance<T>()` usage (if any) → `ReflectionHelper.CreateInstance<T>()`.
- No RhinoMocks mocking in this file — it tests DI infrastructure directly.

#### `CryptographyServiceTests.cs`
- `StubFactory.CreateStub<ConfigurationManagerService>()` → `Substitute.For<ConfigurationManagerService>()`.
- `StubFactory.CreateStub<CertificateProvider>()` → `Substitute.For<CertificateProvider>()`.
- Argument-matched stub setups:
  ```csharp
  // BEFORE
  stubProvider.StubAction(x => x.GetCertificate(Arg.Is(thumbprint))).Return(cert);
  stubProvider.StubAction(x => x.GetCertificate(Arg<string>.Is.Anything)).Throw<NotImplementedException>();

  // AFTER
  stubProvider.GetCertificate(thumbprint).Returns(cert);
  stubProvider.GetCertificate(Arg.Is<string>(s => s != thumbprint)).Throws<NotImplementedException>();
  ```
- Replace `Verify.*` → FluentAssertions.

#### `SecurityAuthorizationContextTests.cs`
- `StubFactory.CreatePrincipal("basic").GrantUserName("testuser")` → `ClaimsPrincipalTestExtensions.CreatePrincipal("basic").GrantUserName("testuser")` (moved helper, same logic).
- `Verify.*` → FluentAssertions.

#### `SecurityAuthorizationPermissionTests.cs`
- `StubFactory.CreatePrincipal(...)` / `.GrantAccessRight(...)` / `.GrantFeatureName(...)` → moved helpers.
- Mixed `Assert.*` calls → FluentAssertions.
- `[ExpectedException(typeof(SecurityAuthorizationException))]` → inline throws.
- `[ExpectedException(typeof(SecurityAuthenticationException))]` → inline throws.

#### `GuidGeneratorServiceTests.cs`
- `Verify.IsNotNull` / `Verify.AreEqual` → FluentAssertions. Minimal changes.

#### `CodeServiceProviderTests.cs`
- `Verify.*` → FluentAssertions.
- `[ExpectedException(typeof(MissingServiceRegistrationException))]` → inline throws.
- `[ExpectedException(typeof(InvalidServiceRegistrationException))]` → inline throws.
- No mocking in this file.

#### `RequiredIfAttributeTests.cs` / `UniqueAttributeTests.cs`
- `Verify.*` → FluentAssertions.
- `[ExpectedException(typeof(ValidationException))]` → inline throws.
- `[ExpectedException(typeof(InvalidOperationException))]` → inline throws.

#### `ObservableModelTest.cs`
- `Verify.*` → FluentAssertions.
- No mocking.

#### `CollectionPropertyMapTest.cs`
- `Verify.*` → FluentAssertions.
- `[ExpectedException(typeof(KeyNotFoundException))]` → inline throws.
- `[ExpectedException(typeof(TargetInvocationException))]` → inline throws.

#### `CompoundDictionaryTest.cs`
- `Verify.*` → FluentAssertions.
- Collection equality assertions → `actual.Should().Equal(expected)` or `BeEquivalentTo(expected)`.

#### `MultiAccessLockTests.cs`
- `Verify.IsInstanceOfType(token, typeof(ReaderAccessToken))` → `token.Should().BeOfType<ReaderAccessToken>()`.
- `[ExpectedException(typeof(LockRecursionException))]` → inline throws.
- `[ExpectedException(typeof(TimeoutException))]` → inline throws.

#### `RegularExpressionTests.cs`
- Already uses `Assert.IsTrue`/`Assert.IsFalse` directly. Replace with FluentAssertions:
  `result.Should().BeTrue()` / `result.Should().BeFalse()`.

#### `ConfidentialControlResolverTests.cs` / `JsonKnownTypeResolverTests.cs`
- `Verify.*` → FluentAssertions. No mocking.

#### `IListExtensionsTest.cs` / `IEnumerableExtensionsTest.cs`
- `Verify.*` → FluentAssertions.
- List equality → `actual.Should().Equal(expected)`.

#### `AzureStorageTableTests.cs` / `AzureServiceBusTopicTests.cs`
- These use hand-written fakes (`AzureStorageTableStub`, `MetadataPropertyTableStub`), not RhinoMocks.
- `ExtendedContext` is already in the UnitTests project — no change.
- Replace `Verify.*` → FluentAssertions.
- Remove `[TestGroup]` import / replace attributes.

---

## 5. Execution Order / Phasing

Suggested order to keep the solution buildable throughout:

| Phase | Work |
|-------|------|
| **1** | Add NSubstitute and FluentAssertions packages to UnitTests. Do NOT yet remove CPP.Framework.Testing reference. Verify build. |
| **2** | Create the `Testing/` helper files (`ServiceStubHelper`, `ReflectionHelper`, `ClaimsPrincipalTestExtensions`, attribute copies). |
| **3** | Rewrite `StubDefaultActions.cs` using NSubstitute and the new helpers. |
| **4** | Rewrite `ConfigSettingsStubExtensions` equivalent (local copy in UnitTests, using real `NameValueCollection`). |
| **5** | Migrate test files **one at a time** — replace `Verify.*`, `[TestGroup]`, `[ExpectedArgument*]`, and `StubFactory` usages. Compile and run tests after each file. |
| **6** | Once all files are migrated and tests pass, remove the `CPP.Framework.Testing` project reference and the `RhinoMocks` package reference. |
| **7** | Remove stale `using` directives (particularly `using CPP.Framework.Diagnostics.Testing;` and `using Rhino.Mocks;`). |
| **8** | Final build + full test run to confirm all tests pass. |

---

## 6. Risks and Considerations

### Non-Virtual Members on Concrete Service Classes
NSubstitute can only intercept `virtual` (or interface-backed) members. If `ConfigurationManagerService`, `FileService`, `CertificateProvider`, or `RoleEnvironmentService` expose non-virtual properties/methods that are currently being stubbed via RhinoMocks, those setups will **silently no-op or throw at runtime** with NSubstitute. Before Phase 3, audit each stubbed type for virtual member coverage. If a member is non-virtual, options are:
- Extract an interface for the service (preferred long-term).
- Use a hand-written test double/fake class.

### `NameValueCollection` Stubbing
As described in §4.4, `NameValueCollection.Get(string)` is non-virtual. The migration switches from stubbing the collection to using a real `NameValueCollection` populated with test values. This changes behavior slightly: the old approach could stub per-key throws; the new approach stubs at the `ConfigurationManagerService` method level. Verify this is semantically equivalent for all tests in `ConfigSettingProviderTests` and `RoleConfigProviderTests`.

### FluentAssertions v7 Breaking Changes
FluentAssertions 7.x introduced several breaking changes from 6.x:
- `AndConstraint` return types changed in some assertion chains.
- Some `BeEquivalentTo` options moved.
- `WithParameterName` is the correct method for `ArgumentException.ParamName` — verify this is available in v7.1.
- Consult the [v7 migration guide](https://fluentassertions.com/upgradingtov7) before finalizing.

### `CallOriginalMethod` on Partial Mocks
The `MethodInvocationExtensions.CallOriginalMethod<T>` helper uses unsafe function-pointer manipulation to call the concrete base implementation from within a RhinoMocks `WhenCalled` handler. With NSubstitute's `Substitute.ForPartsOf<T>()`, the base implementation is called **by default** unless a return value is explicitly configured. Tests that previously used `StubActionContext.CallOriginalMethod()` should simply use `Substitute.ForPartsOf<T>()` without setting up that particular method — the base will be called automatically.

### `ExtendedContext` in `AzureServiceBusTopicTests`
This class extends MSTest's `TestContext` abstract class. It has no testing-framework dependency and is already in the UnitTests project. No change needed.

### `[Ignore]`d Tests
Two tests in `CompoundDictionaryTest` are marked `[Ignore]`. Leave them as-is during migration; they are already excluded from the run.

### Test Running Strategy

`dotnet test` produces no output for net48 projects on Windows — use `vstest.console.exe` from the Visual Studio 2026 installation instead:

```
"C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" ^
  "G:\Framework\main\CPP.Framework.UnitTests\bin\Debug\net48\CPP.Framework.UnitTests.dll"
```

Build first with:
```
dotnet build CPP.Framework.UnitTests/CPP.Framework.UnitTests.csproj -c Debug
```

**Baseline (pre-migration, all passing in Visual Studio 2026):**
- Total: 474 — Passed: 455, Skipped: 19, Failed: 0
- The 19 skipped tests are pre-existing `[Ignore]`-marked tests and are not a concern.

After each migration phase, the pass/skip counts should remain identical and failed should stay at 0.

### `CS0618` Suppression
The project-level `<NoWarn>CS0618</NoWarn>` suppresses obsolete-member warnings. Review after migration whether this suppression is still needed (some RhinoMocks APIs were marked obsolete; once removed, the suppression may be unnecessary).
