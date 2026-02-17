param ([switch]$NoBuild,[switch]$Publish)

if (-not $NoBuild) {
    msbuild CPP.Framework.Libraries.sln /t:Restore
    msbuild CPP.Framework.Libraries.sln /t:Build /p:Configuration=Release

    if (Test-Path .\pkg) {
        Remove-Item .\pkg -Recurse -Force
    }

    New-Item -ItemType Directory -Path .\pkg | Out-Null

    Get-ChildItem -r -fi *.csproj |%  {
        if ($_.Name.StartsWith("CPP.Framework") -and -not $_.FullName.EndsWith("UnitTests.csproj")) {
            .\.nuget\nuget.exe pack $_.FullName -Properties "Configuration=Release" -OutputDirectory pkg    
        }
    }
}

if ($Publish) {
    foreach ($package in Get-ChildItem .\pkg\*.nupkg) {
        .\.nuget\nuget.exe push -Source "cpp.libraries" -ApiKey Az $package
    }
}
