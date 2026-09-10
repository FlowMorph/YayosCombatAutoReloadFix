# Yayo's Combat 3 - Auto Reload Regression Fix

一个面向 RimWorld 1.6 的小型兼容补丁，作者为 [FlowMorph](https://github.com/FlowMorph)。

[Steam 创意工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3799056644) · [GitHub Releases](https://github.com/FlowMorph/YayosCombatAutoReloadFix/releases)

## 中文说明

Yayo's Combat 3 - Addon 1.6.11 删除了 1.6.10 中挂在 `CompApparelReloadable.UsedOnce` 后的自动换弹调用。这个变化会导致两类回归：

- 敌对 Pawn 即使背包里有匹配弹药，也可能在弹匣打空后不再自动换弹。
- 开启 `refillMechAmmo` 时，非 Humanlike Pawn 打空武器后无法进入 Addon 原有的自动补弹流程。

本补丁恢复 1.6.10 的这条调用路径，继续复用 Addon 自己的 `ReloadUtility.TryAutoReloadSingle`、库存弹药与 `refillMechAmmo` 逻辑。它不会增加新的 AI、ThinkTree、弹药生成规则或单位类型判断。

### 依赖与加载顺序

1. [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
2. [Yayo's Combat 3 (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=2854006492)
3. [Yayo's Combat 3 - Addon](https://steamcommunity.com/sharedfiles/filedetails/?id=2796514196)
4. Yayo's Combat 3 - Auto Reload Regression Fix

### 安装

从 Releases 下载压缩包，解压到 RimWorld 的 `Mods` 目录。最终结构应包含 `About/About.xml` 与 `1.6/Assemblies/YayosCombatAutoReloadFix.dll`。

### 源码与构建

项目面向 .NET Framework 4.8。为了避免重新分发上游二进制，仓库不会提交 `Source/References` 中的 DLL。自行构建时，请从本机已安装的两个依赖 Mod 复制 `yayoCombat.dll` 与 `YayosCombatAddon.dll` 到该目录，再构建 `Source/YayosCombatAutoReloadFix.csproj`。

## English

This is a small compatibility patch for RimWorld 1.6. Yayo's Combat 3 - Addon 1.6.11 removed the auto-reload call that ran after `CompApparelReloadable.UsedOnce` in 1.6.10. As a result, hostile pawns may fail to reload ammunition already in their inventory, while non-Humanlike pawns may no longer reach the Addon's existing `refillMechAmmo` behavior after emptying a weapon.

The patch restores the 1.6.10 call path and delegates all behavior to the Addon's existing `ReloadUtility.TryAutoReloadSingle`, inventory-ammo, and `refillMechAmmo` implementations. It adds no new AI, ThinkTree logic, ammunition-generation rules, or pawn classification rules.

Load it after Harmony, Yayo's Combat 3 (Continued), and Yayo's Combat 3 - Addon.

## Compatibility

- RimWorld 1.6
- Designed for Yayo's Combat 3 - Addon 1.6.11
- Source behavior restored from Addon 1.6.10
