# SDK-Style Project Conversion Analysis

## Overview

All 9 projects target **.NET Framework 4.8** and are old-style csproj files. The good news is they've already been partially modernized: all use `<PackageReference>` instead of `packages.config`, and there are no `packages.config` files remaining. Below are the potential issues organized by priority.

---

## 1. MSTest / Visual Studio Test Framework (HIGH - 2 projects)

**Affected:** `CPP.Framework.Testing`, `CPP.Framework.UnitTests`

Both projects use `<Choose>` conditional blocks to reference `Microsoft.VisualStudio.QualityTools.UnitTestFramework` based on Visual Studio version, and import `Microsoft.TestTools.targets`. These are GAC/VS-installed assemblies, not NuGet packages.

**Problem:** SDK-style projects need the `MSTest.TestFramework` and `MSTest.TestAdapter` NuGet packages instead. The `<Choose>` blocks and `Microsoft.TestTools.targets` import must be removed and replaced with PackageReferences.

`CPP.Framework.UnitTests` also has `ProjectTypeGuids` including `{3AC096D0-A1C2-E12C-1390-A8335801FDAB}` (test project GUID), which SDK-style projects don't use.

---

## 2. GAC / Non-NuGet Assembly References (HIGH - 2 projects)

**Affected:** `CPP.Framework.WindowsAzure`, `CPP.Framework.UnitTests`

Both reference `Microsoft.WindowsAzure.ServiceRuntime, Version=2.7.0.0` directly from the GAC (not as a NuGet package). This is the Azure Cloud Service SDK runtime.

**Problem:** SDK-style projects don't work well with GAC references. You'll need to find a NuGet equivalent or include the DLL as a local reference. This assembly is from the legacy Azure Cloud Services SDK and may not have a direct modern NuGet equivalent.

---

## 3. AssemblyInfo.cs Files (MEDIUM - all 9 projects)

Every project has a `Properties\AssemblyInfo.cs` with explicit `<Compile Include>` entries.

**Problem:** SDK-style projects auto-generate assembly attributes by default. You'll either need to set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` or remove the AssemblyInfo.cs files and migrate any custom attributes into the csproj properties. If any AssemblyInfo files contain custom attributes beyond the standard ones (e.g., `InternalsVisibleTo`), those need to be preserved.

---

## 4. Embedded Resource with Code Generator (MEDIUM - 1 project)

**Affected:** `CPP.Framework.Core`

Has an `EmbeddedResource` for `ValidationResources.resx` with `ResXFileCodeGenerator` and a dependent `ValidationResources.Designer.cs`.

**Problem:** This should carry over to SDK-style projects, but the explicit `<Compile Include>` for the `.Designer.cs` with `<AutoGen>`, `<DesignTime>`, and `<DependentUpon>` metadata needs to be preserved rather than relying on default globbing. Verify the designer file regenerates correctly after conversion.

---

## 5. Custom Build Target (MEDIUM - 1 project)

**Affected:** `CPP.Framework.Web`

Has a custom `<Target Name="ChangeAliasesOfConflictedAssemblies">` that sets assembly aliases for `System.Web.Http` to resolve conflicts.

**Problem:** This target must be preserved in the SDK-style csproj. It should work, but test carefully since the timing of `BeforeTargets="FindReferenceAssembliesForReferences;ResolveReferences"` may behave differently in SDK-style builds.

---

## 6. StyleCop Integration (MEDIUM - 1 project)

**Affected:** `CPP.Framework.Core`

References `StyleCop` and `StyleCop.MSBuild` NuGet packages. The solution also has a `Settings.StyleCop` file and `StyleCopTreatErrorsAsWarnings` properties across multiple configurations.

**Problem:** The old StyleCop (5.0) is deprecated and doesn't work well with SDK-style projects. You'd need to migrate to `StyleCop.Analyzers` (Roslyn-based) which uses a different configuration format (`.editorconfig` or `stylecop.json` instead of `Settings.StyleCop`).

---

## 7. NuSpec Files (MEDIUM - 8 projects)

All projects except `UnitTests` have `.nuspec` files for NuGet packaging.

**Problem:** SDK-style projects can generate NuGet packages directly from csproj properties (`<PackageId>`, `<Version>`, `<Authors>`, etc.) using `dotnet pack`. The separate `.nuspec` files would need to be migrated into csproj properties, or you keep them but they can become out of sync with the auto-generated metadata.

---

## 8. Solution-Level NuGet Restore (LOW)

The solution has a `.nuget` folder with `NuGet.Config` and `NuGet.exe` (solution-level NuGet restore from the pre-2015 era). All projects also have `<RestorePackages>true</RestorePackages>`.

**Problem:** SDK-style projects use the modern NuGet restore pipeline. The `.nuget` folder and `RestorePackages` property should be removed.

---

## 9. Custom Configuration Names with Spaces (LOW - all projects)

All projects define configurations like `PDP Dev`, `PDP QA`, `PDP Prod`, `PDP Demo`, and `Demo` in addition to `Debug`/`Release`.

**Problem:** These will work in SDK-style projects but the per-configuration `<PropertyGroup>` blocks with repetitive settings (OutputPath, DocumentationFile, etc.) should be consolidated using MSBuild conditions. The solution configuration mappings also need to be maintained.

---

## 10. System.Web and System.ServiceModel References (LOW)

- `CPP.Framework.EntityData` and `CPP.Framework.Web` reference `System.Web`
- `CPP.Framework.WindowsAzure` and `CPP.Framework.UnitTests` reference `System.ServiceModel`
- `CPP.Framework.Messaging` references `System.DirectoryServices.AccountManagement`

**Problem:** These are .NET Framework-only assemblies. They'll work fine with `net48` target in SDK-style projects, but are a blocker if you ever want to move to .NET 6+/8+ later.

---

## 11. Explicit File Listings (LOW - all projects)

Every project has explicit `<Compile Include>` for every `.cs` file.

**Problem:** SDK-style projects use file globbing by default (`**/*.cs`). The explicit lists can simply be removed. However, verify no files are intentionally excluded.

---

## 12. LangVersion Pinning (LOW - 1 project)

**Affected:** `CPP.Framework.Web` - pins `<LangVersion>7.1</LangVersion>`

**Problem:** Should carry over fine, but worth noting in case you want to uplift the language version consistently across all projects.

---

## Summary - Priority Order

| Priority | Issue | Projects Affected |
|----------|-------|-------------------|
| HIGH | MSTest GAC references + `Choose` blocks | Testing, UnitTests |
| HIGH | Azure ServiceRuntime GAC reference | WindowsAzure, UnitTests |
| MEDIUM | AssemblyInfo.cs auto-generation conflict | All 9 |
| MEDIUM | Resx + Designer code generation | Core |
| MEDIUM | Custom build target (assembly aliasing) | Web |
| MEDIUM | StyleCop 5.0 incompatibility | Core (+ solution) |
| MEDIUM | NuSpec file migration | 8 projects |
| LOW | `.nuget` folder / RestorePackages cleanup | Solution-wide |
| LOW | Custom configuration consolidation | All 9 |
| LOW | Framework-only assembly references | 4 projects |
| LOW | Explicit Compile includes to remove | All 9 |
