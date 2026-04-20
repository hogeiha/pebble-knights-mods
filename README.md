# Pebble Knights Mod 合集

这是为《Pebble Knights》制作的Mod工具合集，包含通用Mod加载器、特性自定义Mod与全员国王动作Mod，支持游戏版本 **v0.1.3**。

---

## 🎮 我想直接用（玩家安装教程）
如果你只是想安装并使用Mod，按以下步骤操作即可，无需阅读后续开发内容。

### 一、前置准备
1.  确认你的游戏版本为 **v0.1.3**（与Mod加载器版本对应）
2.  下载以下文件（从发布页获取）：
    - `BepInEx_win_x64_5.4.22.0.zip` 或同系列 x64 Mono 版（Mod运行前置框架）
    - `PebbleKnights.ModLoader.zip`（游戏专属Mod加载器）
    - （可选）目标Mod文件（如 `CustomTraitFilter` 或 `UniversalKingActions` 完整Mod包）

---

### 二、安装步骤（解压覆盖+文件放置）
#### 步骤1：找到游戏根目录
Steam版本默认路径：
```text
C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights
```
也可以通过Steam客户端快速定位：Steam库 → 右键《Pebble Knights》 → 【管理】 → 【浏览本地文件】。

---

#### 步骤2：安装BepInEx前置框架
1.  将 BepInEx x64 Mono 压缩包全部解压
2.  把解压得到的所有文件/文件夹，**直接覆盖到游戏根目录**
3.  验证安装：游戏根目录出现 `BepInEx` 文件夹、`winhttp.dll`、`doorstop_config.ini` 等文件，说明安装成功。

---

#### 步骤3：安装Pebble Knights Mod加载器
1.  将 `PebbleKnights.ModLoader.zip` 全部解压
2.  把解压得到的文件，**全部覆盖到游戏根目录的 `BepInEx` 文件夹内**
3.  验证安装：路径 `BepInEx/plugins/` 下出现 `PebbleKnights.ModLoader.dll`，说明加载器安装成功。

---

#### 步骤4：创建`Mods`文件夹并安装自定义Mod
1.  在**游戏根目录**下，新建一个名为 `Mods` 的文件夹（如果已有则跳过此步）
2.  将下载的Mod文件夹（如 `CustomTraitFilter`、`UniversalKingActions`）**完整放入 `Mods` 文件夹内**
3.  最终文件结构示例：
    ```text
    Pebble Knights/
      ├─ BepInEx/
      │  └─ plugins/
      │     └─ PebbleKnights.ModLoader.dll
      ├─ Mods/                ← 必须在游戏根目录创建此文件夹
      │  └─ CustomTraitFilter/
      │     ├─ manifest.json
      │     ├─ plugins/
      │     │  └─ CustomTraitFilter.dll
      │     └─ config/
      │        └─ trait-filter.json
      │  └─ UniversalKingActions/
      │     ├─ manifest.json
      │     └─ plugins/
      │        └─ UniversalKingActions.dll
      └─ PebbleKnights.exe
    ```

---

### 三、游戏内使用说明
启动游戏后，可使用以下按键控制Mod功能：
- `F8`：打开/关闭特性自定义面板
- `F9`：刷新特性列表
- `PageUp/PageDown`：上一页/下一页
- `Home/End`：跳转到第一页/最后一页

`UniversalKingActions` 启动后自动生效：所有玩家可以使用国王式抱起/投掷与特性升级行为；真人骑士触发特性升级时由本人选择，AI/bot 触发特性升级时由国王选择；帐篷升级和奖励保留，额外的技能/特性升级篝火入口会被隐藏。

---

## 🛠️ Pebble Knights Mod 工程（开发者说明）

这是 Pebble Knights 的Mod开发工程，目前包含三部分：
```text
PebbleKnights.ModLoader  # 通用 Mod 加载器
CustomTraitFilter        # 特性自定义 Mod
UniversalKingActions     # 全员国王式抱起/投掷与特性升级 Mod
```

### 当前目录结构
```text
pebble-knights-mods/
  src/
    PebbleKnights.ModLoader/
    CustomTraitFilter/
    UniversalKingActions/
  mods/
    CustomTraitFilter/
    UniversalKingActions/
  scripts/
    build.ps1
    install.ps1
    package-release.ps1
  docs/
    从零安装与发布说明.md
```

### 构建Mod
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```
构建产物：
```text
dist/build/PebbleKnights.ModLoader.dll
dist/build/CustomTraitFilter.dll
dist/build/UniversalKingActions.dll
```
`dist/` 为本地生成目录，无需提交到Git仓库。

### 安装到本机游戏
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1
```
默认安装路径：
```text
C:\Program Files (x86)\Steam\steamapps\common\Pebble Knights
```
安装后目录结构与玩家手动安装的标准结构一致。

### 打包发布
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-release.ps1
```
发布产物路径：
```text
dist/release/PebbleKnights-ModLoader-v0.1.0
dist/release/CustomTraitFilter-v0.1.0
dist/release/UniversalKingActions-v0.1.0
dist/release/PebbleKnights-Mods-Bundle-v0.1.0
```

---

## 补充说明
- 完整中文文档：`docs/从零安装与发布说明.md`
- 所有Mod均需通过 `PebbleKnights.ModLoader` 加载，需确保加载器与游戏版本匹配。
