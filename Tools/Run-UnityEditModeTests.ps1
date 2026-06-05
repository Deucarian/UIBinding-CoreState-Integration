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
    "GenericUIItems.CoreState.Tests",
    "-testResults",
    (Join-Path $ProjectPath "GenericUIItems.CoreState.TestResults.xml"),
    "-logFile",
    (Join-Path $ProjectPath "GenericUIItems.CoreState.Unity.log")
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
