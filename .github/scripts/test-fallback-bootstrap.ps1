param(
    [Parameter(Mandatory = $false)]
    [string] $PackageRoot = "."
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $PackageRoot).Path
$manifest =
    Get-Content -LiteralPath (Join-Path $root "package.json") -Raw |
    ConvertFrom-Json
$templatePath = Join-Path $root `
    ".github/fallback-installer/PrefabComponentsBootstrap.cs.template"
$legacyMigrationPath = Join-Path $root `
    "Shared/Authoring/Editor/LegacyScriptsFolderMigration.cs"
$workingRoot = Join-Path ([System.IO.Path]::GetTempPath()) `
    ("wolfy-fallback-bootstrap-compile-" + [guid]::NewGuid().ToString("N"))

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Value
    )

    [System.IO.File]::WriteAllText(
        $Path,
        $Value,
        [System.Text.UTF8Encoding]::new($false)
    )
}

try {
    New-Item -ItemType Directory -Path $workingRoot -Force | Out-Null

    $bootstrap =
        (Get-Content -LiteralPath $templatePath -Raw).Replace(
            "@@BUNDLED_VERSION@@",
            [string] $manifest.version
        )
    Write-Utf8NoBom `
        -Path (Join-Path $workingRoot "PrefabComponentsBootstrap.cs") `
        -Value $bootstrap
    Copy-Item `
        -LiteralPath $legacyMigrationPath `
        -Destination (Join-Path $workingRoot "LegacyScriptsFolderMigration.cs") `
        -Force

    Write-Utf8NoBom `
        -Path (Join-Path $workingRoot "UnityStubs.cs") `
        -Value @'
namespace UnityEngine
{
    public static class Application
    {
        public static string dataPath => "";
    }

    public static class Debug
    {
        public static void Log(object message) {}
        public static void LogError(object message) {}
        public static void LogWarning(object message) {}
    }

    public static class JsonUtility
    {
        public static T FromJson<T>(string json) => default(T);
    }
}

namespace UnityEditor
{
    using System;

    public sealed class PackageInfo {}

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitializeOnLoadAttribute : Attribute {}

    public static class EditorApplication
    {
        public static Action delayCall { get; set; }
        public static event Action update;
        public static event Action projectChanged;
        public static event Action quitting;
        public static double timeSinceStartup => 0;
        public static bool isCompiling => false;
        public static bool isUpdating => false;
    }

    public enum ImportAssetOptions
    {
        Default = 0,
        ForceSynchronousImport = 8
    }

    public static class AssetDatabase
    {
        public static bool IsValidFolder(string path) => false;
        public static string AssetPathToGUID(string path) => "";
        public static bool DeleteAsset(string path) => true;
        public static void Refresh(ImportAssetOptions options) {}
    }
}

namespace UnityEditor.PackageManager
{
    public sealed class PackageInfo
    {
        public string name;
        public static PackageInfo[] GetAllRegisteredPackages() =>
            new PackageInfo[0];
    }

    public static class Client
    {
        public static object Resolve() => null;
    }
}
'@

    Write-Utf8NoBom `
        -Path (Join-Path $workingRoot "BootstrapCompile.csproj") `
        -Value @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
'@

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    $dotnetSdks = if ($null -ne $dotnet) {
        @(& $dotnet.Source --list-sdks)
    }
    else {
        @()
    }

    if ($dotnetSdks.Count -gt 0) {
        & $dotnet.Source build `
            (Join-Path $workingRoot "BootstrapCompile.csproj") `
            --nologo `
            --verbosity quiet
    }
    else {
        $unityCsc = Get-ChildItem `
            -Path "C:\Program Files\Unity\Hub\Editor\*\Editor\Data\MonoBleedingEdge\lib\mono\4.5\csc.exe" `
            -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($null -eq $unityCsc) {
            throw "No .NET SDK or Unity C# compiler was found."
        }

        $monoRoot = Split-Path -Parent (Split-Path -Parent $unityCsc.FullName)
        $monoBleedingEdge = Split-Path -Parent `
            (Split-Path -Parent $monoRoot)
        $monoExecutable = Join-Path $monoBleedingEdge "bin/mono.exe"
        if (-not (Test-Path -LiteralPath $monoExecutable -PathType Leaf)) {
            throw "Unity Mono runtime was not found."
        }

        $compressionReference = Join-Path `
            $monoRoot `
            "4.8-api/System.IO.Compression.dll"
        if (-not (Test-Path -LiteralPath $compressionReference -PathType Leaf)) {
            throw "Unity System.IO.Compression reference was not found."
        }

        $outputAssembly = Join-Path $workingRoot "BootstrapCompile.dll"
        & $monoExecutable `
            $unityCsc.FullName `
            /nologo `
            /target:library `
            /langversion:latest `
            "/out:$outputAssembly" `
            "/reference:$compressionReference" `
            (Join-Path $workingRoot "PrefabComponentsBootstrap.cs") `
            (Join-Path $workingRoot "LegacyScriptsFolderMigration.cs") `
            (Join-Path $workingRoot "UnityStubs.cs")
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Fallback bootstrap compile check failed."
    }

    Write-Host "Fallback bootstrap compile check passed."
}
finally {
    if (Test-Path -LiteralPath $workingRoot) {
        Remove-Item -LiteralPath $workingRoot -Recurse -Force
    }
}
