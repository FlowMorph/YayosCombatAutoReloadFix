# Yayo's Combat 3 - Auto Reload Regression Fix

一个面向 RimWorld 1.6 的小型兼容补丁，作者为 [FlowMorph](https://github.com/FlowMorph)。

[Steam 创意工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3799056644) · [GitHub Releases](https://github.com/FlowMorph/YayosCombatAutoReloadFix/releases)

## 中文说明

Yayo's Combat 3 - Addon 1.6.11 删除了 1.6.10 中挂在 `CompApparelReloadable.UsedOnce` 后的自动换弹调用。这个变化会导致两类回归：

- 敌对 Pawn 即使背包里有匹配弹药，也可能在弹匣打空后不再自动换弹。
- 开启 `refillMechAmmo` 时，非 Humanlike Pawn 打空武器后无法进入 Addon 原有的自动补弹流程。

当非 Humanlike Pawn 仍有少量子弹时，Addon 的低弹自动换弹入口也会被本补丁接管，避免其生成实体弹药后停在原地。虚拟补弹只在完成时填充武器，被打断不会产生或掉落弹药。

本补丁同时解除 Addon NPC 备弹量设置的 500% 上限。设置值仍按百分比解释，例如 1000% 表示按弹匣容量的约十倍生成携带备弹。

本补丁恢复 1.6.10 的这条调用路径，继续复用 Addon 自己的 `ReloadUtility.TryAutoReloadSingle`、库存弹药与 `refillMechAmmo` 逻辑。

另外提供默认开启的“NPC 空枪换弹重试”设置。非玩家人类 Pawn，以及玩家控制的非 Humanlike Pawn，在主武器为空或换弹被中断后，会按照每个 Pawn 各自的 120～180 ticks 周期进行定向检查。背包中存在足量匹配弹药时恢复 Addon 的实体换弹；开启 `refillMechAmmo` 的非 Humanlike Pawn 使用临时虚拟补弹，补弹成功后补满武器，被打断时不会生成或掉落弹药。检查时刻通过 Pawn ID 确定性错开，只检查主武器与背包，不搜索地图弹药，也不修改 AI 或 ThinkTree。玩家人类殖民者不执行这项周期检查。

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

The patch restores the 1.6.10 call path and delegates immediate reload behavior to the Addon's existing `ReloadUtility.TryAutoReloadSingle`, inventory-ammo, and `refillMechAmmo` implementations. Low-ammo non-Humanlike refill cases are intercepted before the Addon's physical generated-ammo fallback, so they receive the same virtual refill path as empty weapons.

It also removes the Addon's 500% NPC carried-ammo setting cap. Values remain percentages of magazine capacity; for example, 1000% requests roughly ten magazines of carried ammunition.

It also adds an optional empty-weapon reload retry, enabled by default. Non-player humanlike pawns and player-controlled non-Humanlike pawns retry every 120–180 ticks after an empty weapon or interrupted reload. Real matching inventory ammunition uses the Addon's reload job; non-Humanlike pawns with `refillMechAmmo` use a temporary virtual refill that fills the weapon on completion and leaves no dropped ammunition when interrupted. Checks are deterministically staggered per pawn, inspect only the primary weapon and inventory, never search the map for ammunition, and do not modify AI or ThinkTrees. Player humanlike colonists are excluded from this periodic check.

Load it after Harmony, Yayo's Combat 3 (Continued), and Yayo's Combat 3 - Addon.

## Compatibility

- RimWorld 1.6
- Designed for Yayo's Combat 3 - Addon 1.6.11
- Source behavior restored from Addon 1.6.10
