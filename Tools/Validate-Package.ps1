$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$requiredFiles = @(
    "package.json",
    ".gitignore",
    "README.md",
    "CHANGELOG.md",
    "LICENSE.md",
    "CONTRIBUTING.md",
    "Runtime/Deucarian.UIBinding.CoreStateBridge.asmdef",
    "Runtime/RepositoryUIBinding.cs",
    "Runtime/SelectionUIBinding.cs",
    "Runtime/ISelectableUIItem.cs",
    "Tests/Editor/Deucarian.UIBinding.CoreStateBridge.Tests.asmdef",
    "Tests/Editor/RepositoryUIBindingTests.cs",
    "Tests/Editor/SelectionUIBindingTests.cs",
    "Samples~/BasicUsage/Deucarian.UIBinding.CoreStateBridge.Samples.BasicUsage.asmdef",
    "Samples~/BasicUsage/Deucarian.UIBinding.CoreStateBridge.Samples.BasicUsage.asmdef.meta",
    "Samples~/BasicUsage/CoreStateUIBindingSample.cs",
    "Samples~/BasicUsage/CoreStateUIBindingSample.cs.meta",
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
if ($package.name -ne "com.deucarian.ui-binding.core-state-bridge") {
    throw "Unexpected package name: $($package.name)"
}

if ($package.version -notmatch "^\d+\.\d+\.\d+$") {
    throw "Package version must be semver MAJOR.MINOR.PATCH: $($package.version)"
}

if ($package.dependencies."com.deucarian.ui-binding" -ne "1.0.0") {
    throw "Expected dependency com.deucarian.ui-binding version 1.0.0"
}

if ($package.dependencies."com.deucarian.core-state" -ne "1.0.0") {
    throw "Expected dependency com.deucarian.core-state version 1.0.0"
}

$runtimeAsmdef = Get-Content -LiteralPath (Join-Path $root "Runtime/Deucarian.UIBinding.CoreStateBridge.asmdef") -Raw | ConvertFrom-Json
if ($runtimeAsmdef.name -ne "Deucarian.UIBinding.CoreStateBridge") {
    throw "Unexpected runtime asmdef name: $($runtimeAsmdef.name)"
}

if ($runtimeAsmdef.references -notcontains "Deucarian.UIBinding") {
    throw "Runtime asmdef must reference Deucarian.UIBinding"
}

if ($runtimeAsmdef.references -notcontains "Deucarian.CoreState") {
    throw "Runtime asmdef must reference Deucarian.CoreState"
}

if ($runtimeAsmdef.references -contains "UnityEditor") {
    throw "Runtime asmdef must not reference UnityEditor"
}

$testAsmdef = Get-Content -LiteralPath (Join-Path $root "Tests/Editor/Deucarian.UIBinding.CoreStateBridge.Tests.asmdef") -Raw | ConvertFrom-Json
foreach ($reference in @("Deucarian.UIBinding.CoreStateBridge", "Deucarian.UIBinding", "Deucarian.CoreState")) {
    if ($testAsmdef.references -notcontains $reference) {
        throw "Tests asmdef must reference $reference"
    }
}

$sampleAsmdef = Get-Content -LiteralPath (Join-Path $root "Samples~/BasicUsage/Deucarian.UIBinding.CoreStateBridge.Samples.BasicUsage.asmdef") -Raw | ConvertFrom-Json
foreach ($reference in @("Deucarian.UIBinding.CoreStateBridge", "Deucarian.UIBinding", "Deucarian.CoreState", "Unity.ugui")) {
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
    if ($content -match "API|ServiceLocator|static\s+readonly\s+Dictionary|static\s+Dictionary") {
        throw "Runtime contains forbidden bridge scope or static cache pattern in $($file.Name)"
    }
}

$uiBindingRoot = "C:/Repositories/UI-Binding"
$coreStateRoot = "C:/Repositories/Core-State"
if (Test-Path -LiteralPath $uiBindingRoot) {
    $uiBindingPackage = Get-Content -LiteralPath (Join-Path $uiBindingRoot "package.json") -Raw
    if ($uiBindingPackage -match "core-state|ui-binding\.core-state-bridge") {
        throw "UIBinding package must not depend on Core State or this bridge package."
    }
}

if (Test-Path -LiteralPath $coreStateRoot) {
    $corePackage = Get-Content -LiteralPath (Join-Path $coreStateRoot "package.json") -Raw
    if ($corePackage -match "ui-binding|ui-binding\.core-state-bridge") {
        throw "Core State package must not depend on UI Binding or this bridge package."
    }
}

$generatedArtifacts = Get-ChildItem -LiteralPath $root -Recurse -Force -File |
    Where-Object { $_.Name -match "\.(unitypackage|zip|tar|tgz)$" }
if ($generatedArtifacts.Count -gt 0) {
    throw "Generated artifacts are present in the package repository."
}

Write-Host "Deucarian.UIBinding.CoreStateBridge package validation passed."
