param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights"
)

$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$BuildDir = Join-Path $Root "dist\build"
$LoaderDll = Join-Path $BuildDir "PebbleKnights.ModLoader.dll"
$ModDll = Join-Path $BuildDir "CustomTraitFilter.dll"
$KingActionsDll = Join-Path $BuildDir "UniversalKingActions.dll"

if (!(Test-Path -LiteralPath $LoaderDll) -or !(Test-Path -LiteralPath $ModDll) -or !(Test-Path -LiteralPath $KingActionsDll)) {
    & (Join-Path $PSScriptRoot "build.ps1") -GameRoot $GameRoot
}

$BepPlugins = Join-Path $GameRoot "BepInEx\plugins"
$ModsRoot = Join-Path $GameRoot "Mods"
$ModRoot = Join-Path $ModsRoot "CustomTraitFilter"
$ModPlugins = Join-Path $ModRoot "plugins"
$ModConfig = Join-Path $ModRoot "config"
$KingActionsRoot = Join-Path $ModsRoot "UniversalKingActions"
$KingActionsPlugins = Join-Path $KingActionsRoot "plugins"

New-Item -ItemType Directory -Force -Path $BepPlugins, $ModPlugins, $ModConfig, $KingActionsPlugins | Out-Null

Copy-Item -LiteralPath $LoaderDll -Destination (Join-Path $BepPlugins "PebbleKnights.ModLoader.dll") -Force
Copy-Item -LiteralPath $ModDll -Destination (Join-Path $ModPlugins "CustomTraitFilter.dll") -Force
Copy-Item -LiteralPath (Join-Path $Root "mods\CustomTraitFilter\manifest.json") -Destination (Join-Path $ModRoot "manifest.json") -Force
Copy-Item -LiteralPath $KingActionsDll -Destination (Join-Path $KingActionsPlugins "UniversalKingActions.dll") -Force
Copy-Item -LiteralPath (Join-Path $Root "mods\UniversalKingActions\manifest.json") -Destination (Join-Path $KingActionsRoot "manifest.json") -Force

$DefaultConfig = Join-Path $Root "mods\CustomTraitFilter\config\trait-filter.json"
$TargetConfig = Join-Path $ModConfig "trait-filter.json"
if (!(Test-Path -LiteralPath $TargetConfig)) {
    Copy-Item -LiteralPath $DefaultConfig -Destination $TargetConfig -Force
}

Write-Host "Installed ModLoader, CustomTraitFilter, and UniversalKingActions to:"
Write-Host "  $GameRoot"
