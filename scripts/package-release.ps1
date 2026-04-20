param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights"
)

$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$ReleaseRoot = Join-Path $Root "dist\release"
$BuildDir = Join-Path $Root "dist\build"

& (Join-Path $PSScriptRoot "build.ps1") -GameRoot $GameRoot

if (Test-Path -LiteralPath $ReleaseRoot) {
    Remove-Item -LiteralPath $ReleaseRoot -Recurse -Force
}

$LoaderPackage = Join-Path $ReleaseRoot "PebbleKnights-ModLoader-v0.1.0"
$TraitPackage = Join-Path $ReleaseRoot "CustomTraitFilter-v0.1.0"
$BundlePackage = Join-Path $ReleaseRoot "PebbleKnights-CustomTraitFilter-Bundle-v0.1.0"

New-Item -ItemType Directory -Force -Path `
    (Join-Path $LoaderPackage "BepInEx\plugins"), `
    (Join-Path $TraitPackage "Mods\CustomTraitFilter\plugins"), `
    (Join-Path $TraitPackage "Mods\CustomTraitFilter\config"), `
    (Join-Path $BundlePackage "BepInEx\plugins"), `
    (Join-Path $BundlePackage "Mods\CustomTraitFilter\plugins"), `
    (Join-Path $BundlePackage "Mods\CustomTraitFilter\config") | Out-Null

Copy-Item -LiteralPath (Join-Path $BuildDir "PebbleKnights.ModLoader.dll") -Destination (Join-Path $LoaderPackage "BepInEx\plugins\PebbleKnights.ModLoader.dll") -Force
Copy-Item -LiteralPath (Join-Path $BuildDir "CustomTraitFilter.dll") -Destination (Join-Path $TraitPackage "Mods\CustomTraitFilter\plugins\CustomTraitFilter.dll") -Force
Copy-Item -LiteralPath (Join-Path $Root "mods\CustomTraitFilter\manifest.json") -Destination (Join-Path $TraitPackage "Mods\CustomTraitFilter\manifest.json") -Force
Copy-Item -LiteralPath (Join-Path $Root "mods\CustomTraitFilter\config\trait-filter.json") -Destination (Join-Path $TraitPackage "Mods\CustomTraitFilter\config\trait-filter.json") -Force

Copy-Item -LiteralPath (Join-Path $LoaderPackage "BepInEx") -Destination $BundlePackage -Recurse -Force
Copy-Item -LiteralPath (Join-Path $TraitPackage "Mods") -Destination $BundlePackage -Recurse -Force

$readme = @"
Pebble Knights 特性自定义 Mod

安装说明：
1. 先把 BepInEx win x64 Mono 安装到 Pebble Knights 游戏根目录。
2. 将本发布包解压到游戏根目录，也就是包含 Pebble Knights.exe 的文件夹。
3. 启动游戏。
4. 按 F8 打开特性自定义面板。

游戏根目录通常是：
C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights

快捷键：
F8：打开/关闭面板
F9：刷新特性列表
PageUp/PageDown：上一页/下一页
Home/End：第一页/最后一页

注意：
本发布包不包含 BepInEx 本体。玩家需要先安装 BepInEx。
完整中文说明见源码工程 docs/从零安装与发布说明.md。
"@

$readme | Set-Content -LiteralPath (Join-Path $LoaderPackage "README.txt") -Encoding UTF8
$readme | Set-Content -LiteralPath (Join-Path $TraitPackage "README.txt") -Encoding UTF8
$readme | Set-Content -LiteralPath (Join-Path $BundlePackage "README.txt") -Encoding UTF8

Write-Host "Release packages created:"
Write-Host "  $LoaderPackage"
Write-Host "  $TraitPackage"
Write-Host "  $BundlePackage"
