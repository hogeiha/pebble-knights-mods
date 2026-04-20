param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights"
)

$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$BuildDir = Join-Path $Root "dist\build"
$LoaderOut = Join-Path $BuildDir "PebbleKnights.ModLoader.dll"
$ModOut = Join-Path $BuildDir "CustomTraitFilter.dll"
$Csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (!(Test-Path -LiteralPath $Csc)) {
    throw "C# compiler not found at $Csc"
}

$BepCore = Join-Path $GameRoot "BepInEx\core"
$Managed = Join-Path $GameRoot "Pebble Knights_Data\Managed"

$Required = @(
    (Join-Path $BepCore "BepInEx.dll"),
    (Join-Path $BepCore "0Harmony.dll"),
    (Join-Path $Managed "UnityEngine.dll"),
    (Join-Path $Managed "UnityEngine.CoreModule.dll"),
    (Join-Path $Managed "UnityEngine.IMGUIModule.dll"),
    (Join-Path $Managed "UnityEngine.InputLegacyModule.dll"),
    (Join-Path $Managed "netstandard.dll")
)

foreach ($Path in $Required) {
    if (!(Test-Path -LiteralPath $Path)) {
        throw "Missing reference: $Path"
    }
}

New-Item -ItemType Directory -Force -Path $BuildDir | Out-Null

$LoaderSources = Get-ChildItem -LiteralPath (Join-Path $Root "src\PebbleKnights.ModLoader") -Filter *.cs | ForEach-Object { $_.FullName }
$ModSources = Get-ChildItem -LiteralPath (Join-Path $Root "src\CustomTraitFilter") -Filter *.cs | ForEach-Object { $_.FullName }

function Invoke-CscChecked {
    param([string[]]$Arguments)

    & $Csc @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "csc failed with exit code $LASTEXITCODE"
    }
}

$LoaderArgs = @(
    "/nologo",
    "/target:library",
    "/optimize+",
    "/debug:pdbonly",
    "/out:$LoaderOut",
    "/reference:$(Join-Path $BepCore "BepInEx.dll")",
    "/reference:$(Join-Path $Managed "UnityEngine.dll")",
    "/reference:$(Join-Path $Managed "UnityEngine.CoreModule.dll")",
    "/reference:$(Join-Path $Managed "netstandard.dll")"
) + $LoaderSources
Invoke-CscChecked -Arguments $LoaderArgs

$ModArgs = @(
    "/nologo",
    "/target:library",
    "/optimize+",
    "/debug:pdbonly",
    "/out:$ModOut",
    "/reference:$LoaderOut",
    "/reference:$(Join-Path $BepCore "BepInEx.dll")",
    "/reference:$(Join-Path $BepCore "0Harmony.dll")",
    "/reference:$(Join-Path $Managed "UnityEngine.dll")",
    "/reference:$(Join-Path $Managed "UnityEngine.CoreModule.dll")",
    "/reference:$(Join-Path $Managed "UnityEngine.IMGUIModule.dll")",
    "/reference:$(Join-Path $Managed "UnityEngine.InputLegacyModule.dll")",
    "/reference:$(Join-Path $Managed "netstandard.dll")"
) + $ModSources
Invoke-CscChecked -Arguments $ModArgs

Write-Host "Built:"
Write-Host "  $LoaderOut"
Write-Host "  $ModOut"
