param(
    [Parameter(Mandatory = $false)]
    [string] $PackageRoot = ".",

    [Parameter(Mandatory = $true)]
    [string] $ExpectedName
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path -LiteralPath $PackageRoot).Path
$manifestPath = Join-Path $root "package.json"
$errors = [System.Collections.Generic.List[string]]::new()

function Get-PackageRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $BasePath,

        [Parameter(Mandatory = $true)]
        [string] $FullPath
    )

    $baseWithSeparator = $BasePath.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    ) + [System.IO.Path]::DirectorySeparatorChar
    $baseUri = [System.Uri]::new($baseWithSeparator)
    $pathUri = [System.Uri]::new($FullPath)
    return [System.Uri]::UnescapeDataString(
        $baseUri.MakeRelativeUri($pathUri).ToString()
    ).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Missing package.json at $manifestPath"
}

try {
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
}
catch {
    throw "package.json is not valid JSON: $($_.Exception.Message)"
}

foreach ($property in @("name", "displayName", "version", "unity", "description", "author")) {
    if ($null -eq $manifest.$property -or [string]::IsNullOrWhiteSpace([string] $manifest.$property)) {
        $errors.Add("package.json is missing required property '$property'.")
    }
}

if ($manifest.name -ne $ExpectedName) {
    $errors.Add("Package name '$($manifest.name)' does not match expected name '$ExpectedName'.")
}

if ([string] $manifest.version -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$') {
    $errors.Add("Package version '$($manifest.version)' is not valid Semantic Versioning.")
}

if ($null -eq $manifest.author -or
    [string]::IsNullOrWhiteSpace([string] $manifest.author.name) -or
    [string]::IsNullOrWhiteSpace([string] $manifest.author.email)) {
    $errors.Add("package.json author must contain both name and email.")
}

if ($ExpectedName -eq "com.wolfy527.prefab-components") {
    $criticalGuids = [ordered]@{
        "Live Mirroring\Runtime\LiveMirroringSystem.cs.meta" = "5c54d508ba4a3ee4baa5148633885b51"
        "Shared\Authoring\Runtime\GeneratedTargetMetadata.cs.meta" = "48742d3549a555842844b99523feab8f"
        "Shared\Authoring\Runtime\GeneratedEditorOnlyObject.cs.meta" = "6a339ada66db0524bb16d5ed1fbe64bc"
        "Shared\Authoring\Runtime\AuthoringOnlyComponent.cs.meta" = "40218417691f9c041a2ac01d1b9d1a5c"
        "Ghost Material.mat.meta" = "4342400023fc9204e9fab7239dec44ef"
        "Paw Heart Logo.png.meta" = "c6672c7f5b5d72b4c9ebf65ad64cfc37"
    }

    foreach ($entry in $criticalGuids.GetEnumerator()) {
        $criticalMetaPath = Join-Path $root $entry.Key
        if (-not (Test-Path -LiteralPath $criticalMetaPath -PathType Leaf)) {
            $errors.Add("Critical compatibility metadata is missing: '$($entry.Key)'.")
            continue
        }

        $criticalGuidLine = Select-String -LiteralPath $criticalMetaPath `
            -Pattern '^guid:\s*([0-9a-fA-F]{32})\s*$' |
            Select-Object -First 1
        $actualCriticalGuid = if ($null -ne $criticalGuidLine) {
            $criticalGuidLine.Matches[0].Groups[1].Value.ToLowerInvariant()
        }
        else {
            ""
        }

        if ($actualCriticalGuid -ne $entry.Value) {
            $errors.Add(
                "Critical GUID changed for '$($entry.Key)'. Expected '$($entry.Value)', found '$actualCriticalGuid'."
            )
        }
    }

    $legacyPath = "Assets\Wolfy_527\~ Supporting Files\Scripts"
    $legacyGuid = "1a754f8d169daa9408e3740cfeeab3aa"
    $legacyProperty = if ($null -ne $manifest.legacyFolders) {
        $manifest.legacyFolders.PSObject.Properties[$legacyPath]
    }
    else {
        $null
    }
    if ($null -eq $legacyProperty -or $legacyProperty.Value -ne $legacyGuid) {
        $errors.Add(
            "package.json must migrate legacy folder '$legacyPath' with GUID '$legacyGuid'."
        )
    }

    $legacyFilePath = "Assets\Wolfy_527\~ Supporting Files\Ghost Material.mat"
    $legacyFileGuid = "4342400023fc9204e9fab7239dec44ef"
    $legacyFileProperty = if ($null -ne $manifest.legacyFiles) {
        $manifest.legacyFiles.PSObject.Properties[$legacyFilePath]
    }
    else {
        $null
    }
    if ($null -eq $legacyFileProperty -or $legacyFileProperty.Value -ne $legacyFileGuid) {
        $errors.Add(
            "package.json must migrate legacy file '$legacyFilePath' with GUID '$legacyFileGuid'."
        )
    }

    foreach ($legacyPackage in @(
        "dev.avatar-tools.prop-components",
        "com.wolfy527.prop-components"
    )) {
        if ($manifest.legacyPackages -notcontains $legacyPackage) {
            $errors.Add(
                "package.json must declare the former package ID '$legacyPackage' in legacyPackages."
            )
        }
    }

    if ($manifest.license -ne "LicenseRef-Wolfy527-Proprietary") {
        $errors.Add(
            "package.json must declare license 'LicenseRef-Wolfy527-Proprietary'."
        )
    }

    if ([string]::IsNullOrWhiteSpace([string] $manifest.licensesUrl)) {
        $errors.Add("package.json must provide licensesUrl.")
    }

    if (-not (Test-Path -LiteralPath (Join-Path $root "LICENSE.md") -PathType Leaf)) {
        $errors.Add("The package must include LICENSE.md.")
    }
}

$ignoredRoots = @(".git", ".github", ".vpm-listing")
$contentFiles = Get-ChildItem -LiteralPath $root -Recurse -File -Force | Where-Object {
    $relative = (Get-PackageRelativePath -BasePath $root -FullPath $_.FullName).Replace('\', '/')
    $topLevel = $relative.Split('/')[0]
    $_.Extension -ne ".meta" -and
    $topLevel -notin $ignoredRoots -and
    $_.Name -notin @(".gitignore", ".gitattributes")
}

foreach ($file in $contentFiles) {
    if (-not (Test-Path -LiteralPath "$($file.FullName).meta" -PathType Leaf)) {
        $relative = Get-PackageRelativePath -BasePath $root -FullPath $file.FullName
        $errors.Add("Unity metadata is missing for '$relative'.")
    }
}

$metaFiles = Get-ChildItem -LiteralPath $root -Recurse -File -Filter "*.meta" -Force | Where-Object {
    $relative = (Get-PackageRelativePath -BasePath $root -FullPath $_.FullName).Replace('\', '/')
    $relative.Split('/')[0] -notin $ignoredRoots
}

$guids = @{}
foreach ($metaFile in $metaFiles) {
    $assetPath = $metaFile.FullName.Substring(0, $metaFile.FullName.Length - 5)
    if (-not (Test-Path -LiteralPath $assetPath)) {
        $relative = Get-PackageRelativePath -BasePath $root -FullPath $metaFile.FullName
        $errors.Add("Orphaned Unity metadata '$relative'.")
    }

    $guidLine = Select-String -LiteralPath $metaFile.FullName -Pattern '^guid:\s*([0-9a-fA-F]{32})\s*$' | Select-Object -First 1
    if ($null -eq $guidLine) {
        $relative = Get-PackageRelativePath -BasePath $root -FullPath $metaFile.FullName
        $errors.Add("Unity metadata '$relative' does not contain a valid GUID.")
        continue
    }

    $guid = $guidLine.Matches[0].Groups[1].Value.ToLowerInvariant()
    if ($guids.ContainsKey($guid)) {
        $first = Get-PackageRelativePath -BasePath $root -FullPath $guids[$guid]
        $second = Get-PackageRelativePath -BasePath $root -FullPath $metaFile.FullName
        $errors.Add("Duplicate Unity GUID '$guid' in '$first' and '$second'.")
    }
    else {
        $guids[$guid] = $metaFile.FullName
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Host "ERROR: $_" -ForegroundColor Red }
    throw "Package validation failed with $($errors.Count) error(s)."
}

Write-Host "Validated $($manifest.displayName) $($manifest.version): $($contentFiles.Count) assets and $($metaFiles.Count) metadata files."
