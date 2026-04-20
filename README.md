# Pebble Knights Mod 工程

这是 Pebble Knights 的mod 工程，目前包含两部分：

```text
PebbleKnights.ModLoader  # 通用 mod 加载器
CustomTraitFilter        # 特性自定义 mod
```

## 当前目录

```text
pebble-knights-mods/
  src/
    PebbleKnights.ModLoader/
    CustomTraitFilter/
  mods/
    CustomTraitFilter/
  scripts/
    build.ps1
    install.ps1
    package-release.ps1
  docs/
    从零安装与发布说明.md
```

## 构建

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

构建产物：

```text
dist/build/PebbleKnights.ModLoader.dll
dist/build/CustomTraitFilter.dll
```

`dist/` 是本地生成目录，不需要提交到 GitHub。

## 安装到本机游戏

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1
```

默认安装到：

```text
C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights
```

安装后结构：

```text
Pebble Knights/
  BepInEx/plugins/PebbleKnights.ModLoader.dll
  Mods/CustomTraitFilter/
    manifest.json
    plugins/CustomTraitFilter.dll
    config/trait-filter.json
```

## 打包发布

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-release.ps1
```

发布目录：

```text
dist/release/PebbleKnights-ModLoader-v0.1.0
dist/release/CustomTraitFilter-v0.1.0
dist/release/PebbleKnights-CustomTraitFilter-Bundle-v0.1.0
```

## 玩家说明

完整中文说明：

```text
docs/从零安装与发布说明.md
```

游戏内使用：

```text
F8：打开/关闭特性自定义面板
F9：刷新特性列表
PageUp/PageDown：上一页/下一页
Home/End：第一页/最后一页
```
