$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$requiredFiles = @(
    "package.json",
    ".gitignore",
    "README.md",
    "CHANGELOG.md",
    "LICENSE.md",
    "CONTRIBUTING.md",
    "Runtime/GenericUIItems.CoreState.asmdef",
    "Runtime/RepositoryUIBinding.cs",
    "Runtime/SelectionUIBinding.cs",
    "Runtime/ISelectableUIItem.cs",
    "Tests/Editor/GenericUIItems.CoreState.Tests.asmdef",
    "Tests/Editor/RepositoryUIBindingTests.cs",
    "Tests/Editor/SelectionUIBindingTests.cs",
    "Samples~/BasicUsage/GenericUIItems.CoreState.Samples.BasicUsage.asmdef",
    "Samples~/BasicUsage/GenericUIItems.CoreState.Samples.BasicUsage.asmdef.meta",
    "Samples~/BasicUsage/CoreStateGenericUIItemsSample.cs",
    "Samples~/BasicUsage/CoreStateGenericUIItemsSample.cs.meta",
    "Samples~/BasicUsage/CoreStateSampleItem.cs",
    "Samples~/BasicUsage/CoreStateSampleItem.cs.meta",
    "Samples~/BasicUsage/CoreStateSampleItemData.cs",
    "Samples~/BasicUsage/CoreStateSampleItemData.cs.meta",
    "Samples~/BasicUsage/README.md",
    "Samples~/BasicUsage/README.md.meta",
    "Tools/Run-UnityEditModeTests.ps1"
)

$requiredDirectories = @(
    "Runtime",
    "Tests/Editor",
    "Samples~/BasicUsage",
    "Tools"
)

foreach ($directory in $requiredDirectories) {
    $path = Join-Path $root $directory
    if (-not (Test-Path -LiteralPath $path -PathType Container)) {
        throw "Missing required directory: $directory"
    }
}

foreach ($file in $requiredFiles) {
    $path = Join-Path $root $file
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required file: $file"
    }
}

$package = Get-Content -LiteralPath (Join-Path $root "package.json") -Raw | ConvertFrom-Json
if ($package.name -ne "com.jorishoef.generic-ui-items-core-state") {
    throw "Unexpected package name: $($package.name)"
}

if ($package.version -notmatch "^\d+\.\d+\.\d+$") {
    throw "Package version must be semver MAJOR.MINOR.PATCH: $($package.version)"
}

if ($package.dependencies."com.jorishoef.generic-ui-items" -ne "1.0.0") {
    throw "Expected dependency com.jorishoef.generic-ui-items version 1.0.0"
}

if ($package.dependencies."com.jorishoef.core.state" -ne "1.0.0") {
    throw "Expected dependency com.jorishoef.core.state version 1.0.0"
}

$runtimeAsmdef = Get-Content -LiteralPath (Join-Path $root "Runtime/GenericUIItems.CoreState.asmdef") -Raw | ConvertFrom-Json
if ($runtimeAsmdef.name -ne "GenericUIItems.CoreState") {
    throw "Unexpected runtime asmdef name: $($runtimeAsmdef.name)"
}

if ($runtimeAsmdef.references -notcontains "GenericUIItems") {
    throw "Runtime asmdef must reference GenericUIItems"
}

if ($runtimeAsmdef.references -notcontains "JorisHoef.Core.State") {
    throw "Runtime asmdef must reference JorisHoef.Core.State"
}

if ($runtimeAsmdef.references -contains "UnityEditor") {
    throw "Runtime asmdef must not reference UnityEditor"
}

$testAsmdef = Get-Content -LiteralPath (Join-Path $root "Tests/Editor/GenericUIItems.CoreState.Tests.asmdef") -Raw | ConvertFrom-Json
foreach ($reference in @("GenericUIItems.CoreState", "GenericUIItems", "JorisHoef.Core.State")) {
    if ($testAsmdef.references -notcontains $reference) {
        throw "Tests asmdef must reference $reference"
    }
}

$sampleAsmdef = Get-Content -LiteralPath (Join-Path $root "Samples~/BasicUsage/GenericUIItems.CoreState.Samples.BasicUsage.asmdef") -Raw | ConvertFrom-Json
foreach ($reference in @("GenericUIItems.CoreState", "GenericUIItems", "JorisHoef.Core.State", "Unity.ugui")) {
    if ($sampleAsmdef.references -notcontains $reference) {
        throw "Sample asmdef must reference $reference"
    }
}

$forbiddenProjectScaffolding = @("Assets", "Packages", "ProjectSettings")
foreach ($directory in $forbiddenProjectScaffolding) {
    $path = Join-Path $root $directory
    if (Test-Path -LiteralPath $path -PathType Container) {
        throw "Package repository should not contain Unity project scaffolding directory: $directory"
    }
}

$runtimeFiles = Get-ChildItem -LiteralPath (Join-Path $root "Runtime") -Recurse -File
foreach ($file in $runtimeFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ($content -match "APIHelper|ServiceLocator|static\s+readonly\s+Dictionary|static\s+Dictionary") {
        throw "Runtime contains forbidden integration scope or static cache pattern in $($file.Name)"
    }
}

$genericUIItemsRoot = "C:/Repositories/GenericUIItems"
$coreStateRoot = "C:/Repositories/Core-State"
if (Test-Path -LiteralPath $genericUIItemsRoot) {
    $genericPackage = Get-Content -LiteralPath (Join-Path $genericUIItemsRoot "package.json") -Raw
    if ($genericPackage -match "core.state|generic-ui-items-core-state") {
        throw "GenericUIItems package must not depend on Core State or this integration package."
    }
}

if (Test-Path -LiteralPath $coreStateRoot) {
    $corePackage = Get-Content -LiteralPath (Join-Path $coreStateRoot "package.json") -Raw
    if ($corePackage -match "generic-ui-items|generic-ui-items-core-state") {
        throw "Core State package must not depend on Generic UI Items or this integration package."
    }
}

$generatedArtifacts = Get-ChildItem -LiteralPath $root -Recurse -Force -File |
    Where-Object { $_.Name -match "\.(unitypackage|zip|tar|tgz)$" }
if ($generatedArtifacts.Count -gt 0) {
    throw "Generated artifacts are present in the package repository."
}

Write-Host "GenericUIItems.CoreState package validation passed."
