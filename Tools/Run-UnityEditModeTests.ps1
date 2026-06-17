param(
    [Parameter(Mandatory = $true)]
    [string] $UnityEditorPath,

    [Parameter(Mandatory = $true)]
    [string] $ProjectPath
)

$ErrorActionPreference = "Stop"

$arguments = @(
    "-batchmode",
    "-nographics",
    "-projectPath",
    $ProjectPath,
    "-runTests",
    "-testPlatform",
    "editmode",
    "-assemblyNames",
    "Deucarian.UIBinding.CoreStateIntegration.Tests",
    "-testResults",
    (Join-Path $ProjectPath "Deucarian.UIBinding.CoreStateIntegration.TestResults.xml"),
    "-logFile",
    (Join-Path $ProjectPath "Deucarian.UIBinding.CoreStateIntegration.Unity.log")
)

$process = Start-Process `
    -FilePath $UnityEditorPath `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden

if ($process.ExitCode -ne 0) {
    throw "Unity EditMode tests failed with exit code $($process.ExitCode)"
}
