# SDK-Style Project Conversion Guide

## Overview

All 9 projects target **.NET Framework 4.8** and use old-style csproj files. They have already been partially modernized (all use `<PackageReference>` instead of `packages.config`). This document provides a repeatable procedure for converting each project to SDK-style, plus a per-project reference of specific details needed during conversion.

**Completed:** CPP.Framework.Core, CPP.Framework.Serialization, CPP.Framework.Messaging, CPP.Framework.Web, CPP.Framework.EntityData, CPP.Framework.WindowsAzure, CPP.Framework.WindowsAzure.ApplicationInsights

---

## General Conversion Procedure

Apply these steps to each project. Refer to the per-project reference section below for the specific values.

### Step 1: Rewrite the csproj as SDK-style

Replace the entire csproj with a new SDK-style file structured as follows:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <RootNamespace><!-- from old csproj --></RootNamespace>
    <AssemblyName><!-- from old csproj --></AssemblyName>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>CS0618</NoWarn>

    <!-- Assembly metadata (migrated from AssemblyInfo.cs) -->
    <Title><!-- from AssemblyTitle --></Title>
    <Description><!-- from AssemblyDescription --></Description>
    <Company><!-- from AssemblyCompany --></Company>
    <Product><!-- from AssemblyProduct --></Product>
    <Copyright><!-- from AssemblyCopyright --></Copyright>
    <AssemblyVersion><!-- from AssemblyVersion --></AssemblyVersion>
    <FileVersion><!-- from AssemblyFileVersion --></FileVersion>
    <InformationalVersion><!-- from AssemblyInformationalVersion --></InformationalVersion>

    <!-- NuGet package (migrated from nuspec) -->
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <Version><!-- AssemblyInformationalVersion + "-alpha" --></Version>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageReleaseNotes><!-- from nuspec releaseNotes --></PackageReleaseNotes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <!-- InternalsVisibleTo (if any in AssemblyInfo.cs) -->
  <ItemGroup>
    <InternalsVisibleTo Include="..." />
  </ItemGroup>

  <!-- Framework references: only keep non-implicit ones -->
  <ItemGroup>
    <Reference Include="..." />
  </ItemGroup>

  <!-- PackageReferences: carry over from old csproj, minus StyleCop -->
  <ItemGroup>
    <PackageReference Include="..." Version="..." />
  </ItemGroup>

</Project>
```

### Step 2: Suppress warning CS0618

Add `<NoWarn>CS0618</NoWarn>` to the main `<PropertyGroup>`. This suppresses "member is obsolete" warnings that are pervasive across the codebase (internal deprecated APIs that haven't been cleaned up yet). These warnings are pre-existing noise and not actionable during the migration.

### Step 3: Remove items handled by SDK defaults

The following items from the old csproj should NOT be carried over:

- **All `<Compile Include>` items** — SDK auto-globs `**/*.cs`
- **All per-configuration `<PropertyGroup>` blocks** — SDK defaults handle Debug/Release; custom configs (PDP Dev/QA/Prod/Demo, Demo) are defined at the solution level and inherit sensible defaults
- **`<GenerateDocumentationFile>true</GenerateDocumentationFile>`** replaces per-config `<DocumentationFile>` paths
- `ProjectGuid`, `FileAlignment`, `AppDesignerFolder`, `OutputType` (Library is the default)
- `SolutionDir`, `RestorePackages`, `RestoreProjectStyle`, `NuGetPackageImportStamp`
- `TargetFrameworkProfile`
- All `StyleCopTreatErrorsAsWarnings` properties
- Empty `PostBuildEvent`
- Commented-out BeforeBuild/AfterBuild targets
- `<Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />`
- `<None Include="*.nuspec">` items

### Step 4: Keep only non-implicit framework references

Remove these implicit framework references (SDK includes them automatically):
- `System`, `System.Core`, `System.Data`, `System.Xml`, `System.Xml.Linq`
- `System.Data.DataSetExtensions`, `Microsoft.CSharp`, `System.Net.Http`

Keep any others that the project actually uses, e.g.:
- `System.Configuration`
- `System.ComponentModel.DataAnnotations`
- `System.Web`
- `System.Runtime.Serialization`
- `System.ServiceModel`
- `System.DirectoryServices.AccountManagement`

### Step 5: Remove StyleCop packages

If the project has `StyleCop` or `StyleCop.MSBuild` PackageReferences, remove them. StyleCop 5.0 is deprecated and incompatible with SDK-style projects.

### Step 6: Delete AssemblyInfo.cs

After migrating all attributes into the csproj (Step 1), delete `Properties\AssemblyInfo.cs`. If the `Properties\` directory is now empty, delete it too.

Attribute mapping:
| AssemblyInfo.cs attribute | csproj property |
|---------------------------|-----------------|
| `AssemblyTitle` | `<Title>` |
| `AssemblyDescription` | `<Description>` |
| `AssemblyCompany` | `<Company>` |
| `AssemblyProduct` | `<Product>` |
| `AssemblyCopyright` | `<Copyright>` |
| `AssemblyVersion` | `<AssemblyVersion>` |
| `AssemblyFileVersion` | `<FileVersion>` |
| `AssemblyInformationalVersion` | `<InformationalVersion>` |
| `InternalsVisibleTo` | `<InternalsVisibleTo Include="...">` item |

These attributes are dropped (not needed):
- `AssemblyConfiguration`, `AssemblyTrademark`, `AssemblyCulture`
- `ComVisible`, `Guid`

### Step 7: Delete the .nuspec file

After migrating metadata into the csproj (Step 1), delete the `.nuspec` file. NuGet package dependencies auto-flow from `<PackageReference>` items, and framework assembly dependencies are covered by `<Reference>` items — no need to list them explicitly.

### Step 8: Check for excluded source files

SDK-style projects auto-glob all `**/*.cs` files. If the old csproj intentionally excluded any `.cs` files (i.e., they exist on disk but are not in any `<Compile Include>`), either:
- **Delete them** if they are no longer needed, or
- **Add `<Compile Remove="path" />`** to exclude them from compilation

### Step 9: Preserve special items

Some projects have items that must be carried over:

- **EmbeddedResource with designer codegen** (resx files): Use `Update` syntax since files are auto-discovered:
  ```xml
  <EmbeddedResource Update="path\to\Resource.resx">
    <Generator>ResXFileCodeGenerator</Generator>
    <LastGenOutput>Resource.Designer.cs</LastGenOutput>
  </EmbeddedResource>
  <Compile Update="path\to\Resource.Designer.cs">
    <AutoGen>True</AutoGen>
    <DesignTime>True</DesignTime>
    <DependentUpon>Resource.resx</DependentUpon>
  </Compile>
  ```

- **Custom build targets**: Copy them as-is into the new csproj.

- **Content files** (e.g., `CPPEncryption.pfx` with CopyToOutputDirectory): Preserve using `Update` or explicit `Include`.

- **Project references**: Carry over `<ProjectReference>` items as-is.

- **app.config files**: SDK-style projects auto-detect `app.config` — no explicit `<None Include>` needed.

### Step 10: Create a README.md

For projects that generate a NuGet package (`GeneratePackageOnBuild`), create a `README.md` in the project root with a brief description and feature list. The csproj template (Step 1) already includes `<PackageReadmeFile>` and the `<None Include="README.md" Pack="true" .../>` item to include it in the package.

### Step 11: Build and verify

```
dotnet build <project>.csproj --configuration Debug
dotnet build <project>.csproj --configuration Release
```

Verify:
- 0 errors (warnings are expected — they are pre-existing)
- XML documentation file is generated (in `bin\<config>\net48\`)
- NuGet package `.nupkg` is generated (in `bin\<config>\`)
- Any resx designer files compile correctly

---

## Per-Project Reference

### CPP.Framework.Core — DONE

Converted. See `CPP.Framework.Core\CPP.Framework.Core.csproj` for the reference template.

---

### CPP.Framework.EntityData — DONE

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework` |
| AssemblyName | `CPP.Framework.EntityData` |
| AssemblyVersion | `2.0.4.0` |
| FileVersion | `2.0.4.0` |
| InformationalVersion | `2.0.4` |
| Version (NuGet) | `2.0.4-alpha` |
| PackageReleaseNotes | Fixed a bug in the Entity Framework data source context that prevents SaveChanges() from returning successfully when an entity is being deleted, even though the database operation completed successfully. |

**InternalsVisibleTo:** DynamicProxyGenAssembly2, CPP.Framework.UnitTests, CPP.Elevate.BusinessServices.UnitTests, CPP.Platform.Data.Objects

**Framework References (non-implicit):**
- System.ComponentModel.DataAnnotations
- System.Configuration
- System.Web

**PackageReferences:**
| Package | Version |
|---------|---------|
| EntityFramework | 6.4.4 |

**Special:** Has a project reference to CPP.Framework.Core. No excluded files.

---

### CPP.Framework.Serialization — DONE

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework` |
| AssemblyName | `CPP.Framework.Serialization` |
| AssemblyVersion | `2.0.3.0` |
| FileVersion | `2.0.3.0` |
| InformationalVersion | `2.0.3` |
| Version (NuGet) | `2.0.3-alpha` |
| PackageReleaseNotes | Fixed the dependency list for the package. |

**InternalsVisibleTo:** None

**Framework References (non-implicit):**
- System.Runtime.Serialization

**PackageReferences:**
| Package | Version |
|---------|---------|
| Newtonsoft.Json | 13.0.3 |
| protobuf-net | 2.0.0.668 |

**Special:** No excluded files. Straightforward conversion.

---

### CPP.Framework.Messaging — DONE

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework.Messaging` |
| AssemblyName | `CPP.Framework.Messaging` |
| AssemblyVersion | `2.0.3.0` |
| FileVersion | `2.0.3.0` |
| InformationalVersion | `2.0.3` |
| Version (NuGet) | `2.0.3-alpha` |
| PackageReleaseNotes | Added the XML documentation files for the library to make them available for Visual Studio IntelliSense. |

**InternalsVisibleTo:** None

**Framework References (non-implicit):**
- System.DirectoryServices.AccountManagement

**PackageReferences:**
| Package | Version |
|---------|---------|
| SendGrid | 1.0.2 |

**Special:** No excluded files. Straightforward conversion.

---

### CPP.Framework.Web — DONE

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework.Web` |
| AssemblyName | `CPP.Framework.Web` |
| AssemblyVersion | `2.0.4.0` |
| FileVersion | `2.0.4.0` |
| InformationalVersion | `2.0.4` |
| Version (NuGet) | `2.0.4-alpha` |
| PackageReleaseNotes | Fixed a bug related to SecurityAuthorizeAttribute not working correctly for MVC controllers and action methods. |

**InternalsVisibleTo:** None

**Framework References (non-implicit):**
- System.Web

**PackageReferences:**
| Package | Version |
|---------|---------|
| Microsoft.AspNet.Mvc | 5.2.9 |
| Microsoft.AspNet.WebApi | 5.2.9 |
| Microsoft.AspNet.WebApi.Client | 5.2.9 |
| Microsoft.AspNet.WebApi.Versioning | 2.3.0 |
| Newtonsoft.Json | 13.0.3 |

**Special — custom build target (must preserve):**
```xml
<Target Name="ChangeAliasesOfConflictedAssemblies" BeforeTargets="FindReferenceAssembliesForReferences;ResolveReferences">
  <ItemGroup>
    <ReferencePath Condition="'%(FileName)' == 'System.Web.Http'">
      <Aliases>httpAlias</Aliases>
    </ReferencePath>
  </ItemGroup>
</Target>
```

**Special — LangVersion:** The old csproj pins `<LangVersion>7.1</LangVersion>` (Debug only). Carry over as a project-wide property if desired, or drop it to use the SDK default.

No excluded files.

---

### CPP.Framework.WindowsAzure — DONE

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework.WindowsAzure` |
| AssemblyName | `CPP.Framework.WindowsAzure` |
| AssemblyVersion | `2.0.7.0` |
| FileVersion | `2.0.7.0` |
| InformationalVersion | `2.0.7` |
| Version (NuGet) | `2.0.7-alpha` |
| PackageReleaseNotes | Added the ability to indicate the formatted value should be lowercased when specifying a custom property for a service bus message. |

**InternalsVisibleTo:** DynamicProxyGenAssembly2, CPP.Framework.UnitTests, CPP.Elevate.BusinessServices.IntegrationTests, CPP.Elevate.BusinessServices.UnitTests

**Framework References (non-implicit):**
- System.Configuration
- System.Runtime.Serialization
- System.ServiceModel

**GAC Reference (non-NuGet):**
- `Microsoft.WindowsAzure.ServiceRuntime, Version=2.7.0.0` — Resolved using a local copy at `..\lib\Microsoft.WindowsAzure.ServiceRuntime.dll` (checked into the repo under `lib\`).

**PackageReferences:**
| Package | Version |
|---------|---------|
| JetBrains.Annotations | 11.1.0 |
| Microsoft.Azure.WebJobs | 1.1.1 |
| Microsoft.Azure.WebJobs.ServiceBus | 1.1.1 |
| Microsoft.WindowsAzure.ConfigurationManager | 3.2.1 |
| Newtonsoft.Json | 13.0.3 |
| WindowsAzure.ServiceBus | 2.7.6 |
| WindowsAzure.Storage | 4.3.0 |

**Special:** Has a project reference to CPP.Framework.WindowsAzure.ApplicationInsights. Has app.config. No excluded files.

---

### CPP.Framework.WindowsAzure.ApplicationInsights — DONE

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework.WindowsAzure.ApplicationInsights` |
| AssemblyName | `CPP.Framework.WindowsAzure.ApplicationInsights` |
| AssemblyVersion | `2.0.2.0` |
| FileVersion | `2.0.2.0` |
| InformationalVersion | `2.0.2` |
| Version (NuGet) | `2.0.2-alpha` |
| PackageReleaseNotes | Remove the orphaned dependency on Live Snapshot Debugging from the package definition. |

**InternalsVisibleTo:** DynamicProxyGenAssembly2, CPP.Framework.UnitTests, CPP.Elevate.BusinessServices.UnitTests

**Framework References (non-implicit):**
- System.Net.Http *(note: this is implicit in SDK-style — may not need to be listed)*

**PackageReferences:**
| Package | Version |
|---------|---------|
| Microsoft.ApplicationInsights | 2.9.1 |
| Microsoft.ApplicationInsights.PerfCounterCollector | 2.9.1 |

**Special:** No excluded files. Straightforward conversion.

---

### CPP.Framework.Testing

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework.Testing` |
| AssemblyName | `CPP.Framework.Testing` |
| AssemblyVersion | `2.0.1.0` |
| FileVersion | `2.0.1.0` |
| InformationalVersion | `2.0.1` |
| Version (NuGet) | `2.0.1-alpha` |
| PackageReleaseNotes | Added a helper extension method to allow a stub to call it's original method implementation. |

**InternalsVisibleTo:** DynamicProxyGenAssembly2, CPP.Framework.UnitTests, CPP.Elevate.BusinessServices.UnitTests

**Framework References (non-implicit):**
- System.Configuration

**PackageReferences:**
| Package | Version |
|---------|---------|
| JetBrains.Annotations | 11.1.0 |
| RhinoMocks | 3.6.1 |

**Special — MSTest migration required:**
The old csproj has `<Choose>` conditional blocks referencing `Microsoft.VisualStudio.QualityTools.UnitTestFramework` from the GAC based on VS version. These must be removed and replaced with MSTest NuGet packages:
```xml
<PackageReference Include="MSTest.TestFramework" Version="3.6.3" />
```
Remove all `<Choose>` blocks and the `Microsoft.TestTools.targets` import.

No excluded files.

---

### CPP.Framework.UnitTests

| Field | Value |
|-------|-------|
| RootNamespace | `CPP.Framework.UnitTests` |
| AssemblyName | `CPP.Framework.UnitTests` |
| AssemblyVersion | `1.0.0.0` |
| FileVersion | `1.0.0.0` |
| InformationalVersion | *(not set — use `1.0.0`)* |
| GeneratePackageOnBuild | **false** (no nuspec — this is a test project) |
| GenerateDocumentationFile | **false** (old csproj does not generate XML docs) |

**InternalsVisibleTo:** None

**Framework References (non-implicit):**
- System.Configuration
- System.Runtime.Serialization
- System.ServiceModel

**GAC Reference (non-NuGet):**
- `Microsoft.WindowsAzure.ServiceRuntime, Version=2.7.0.0` — same issue as CPP.Framework.WindowsAzure

**PackageReferences:**
| Package | Version |
|---------|---------|
| JetBrains.Annotations | 11.1.0 |
| Microsoft.Data.Edm | 5.6.4 |
| Microsoft.Data.OData | 5.6.4 |
| Microsoft.Data.Services.Client | 5.6.4 |
| Microsoft.WindowsAzure.ConfigurationManager | 3.2.1 |
| Newtonsoft.Json | 13.0.3 |
| RhinoMocks | 3.6.1 |
| System.Spatial | 5.6.2 |
| WindowsAzure.ServiceBus | 2.7.6 |
| WindowsAzure.Storage | 4.3.0 |

**Special — MSTest migration required:**
Same as CPP.Framework.Testing: remove `<Choose>` blocks and `Microsoft.TestTools.targets` import, add MSTest NuGet packages:
```xml
<PackageReference Include="MSTest.TestFramework" Version="3.6.3" />
<PackageReference Include="MSTest.TestAdapter" Version="3.6.3" />
```

**Special — content file:**
`CPPEncryption.pfx` must be preserved with CopyToOutputDirectory:
```xml
<None Update="CPPEncryption.pfx">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

**Special:** Remove `ProjectTypeGuids` (not used in SDK-style). Has multiple project references to other framework projects. No excluded files.

---

## Suggested Conversion Order

1. ~~CPP.Framework.Core~~ — **Done**
2. ~~CPP.Framework.Serialization~~ — **Done**
3. ~~CPP.Framework.Messaging~~ — **Done**
4. ~~CPP.Framework.WindowsAzure.ApplicationInsights~~ — **Done**
5. ~~CPP.Framework.EntityData~~ — **Done**
6. ~~CPP.Framework.Web~~ — **Done**
7. ~~CPP.Framework.WindowsAzure~~ — **Done**
8. CPP.Framework.Testing — has MSTest Choose blocks to migrate
9. CPP.Framework.UnitTests — has MSTest + GAC reference + most project references; do last

---

## Solution-Level Cleanup (after all projects converted)

- Delete the `.nuget` folder (contains legacy NuGet.exe and NuGet.Config for solution-level restore)
- Delete `Settings.StyleCop` if present at the solution root
- Verify all solution configurations (PDP Dev/QA/Prod/Demo, Demo) still build correctly
