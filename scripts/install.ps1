param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights"
)

$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$BuildDir = Join-Path $Root "dist\build"
$LoaderDll = Join-Path $BuildDir "PebbleKnights.ModLoader.dll"
$ModDll = Join-Path $BuildDir "CustomTraitFilter.dll"

if (!(Test-Path -LiteralPath $LoaderDll) -or !(Test-Path -LiteralPath $ModDll)) {
    & (Join-Path $PSScriptRoot "build.ps1") -GameRoot $GameRoot
}

$BepPlugins = Join-Path $GameRoot "BepInEx\plugins"
$ModsRoot = Join-Path $GameRoot "Mods"
$ModRoot = Join-Path $ModsRoot "CustomTraitFilter"
$ModPlugins = Join-Path $ModRoot "plugins"
$ModConfig = Join-Path $ModRoot "config"

New-Item -ItemType Directory -Force -Path $BepPlugins, $ModPlugins, $ModConfig | Out-Null

Copy-Item -LiteralPath $LoaderDll -Destination (Join-Path $BepPlugins "PebbleKnights.ModLoader.dll") -Force
Copy-Item -LiteralPath $ModDll -Destination (Join-Path $ModPlugins "CustomTraitFilter.dll") -Force
Copy-Item -LiteralPath (Join-Path $Root "mods\CustomTraitFilter\manifest.json") -Destination (Join-Path $ModRoot "manifest.json") -Force

$DefaultConfig = Join-Path $Root "mods\CustomTraitFilter\config\trait-filter.json"
$TargetConfig = Join-Path $ModConfig "trait-filter.json"
if (!(Test-Path -LiteralPath $TargetConfig)) {
    Copy-Item -LiteralPath $DefaultConfig -Destination $TargetConfig -Force
}

Write-Host "Installed ModLoader and CustomTraitFilter to:"
Write-Host "  $GameRoot"
