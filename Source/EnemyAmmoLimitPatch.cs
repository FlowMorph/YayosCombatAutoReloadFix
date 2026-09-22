using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace YayosCombatAutoReloadFix
{
    [HarmonyPatch(typeof(yayoCombat.YayoCombatMod), nameof(yayoCombat.YayoCombatMod.DoSettingsWindowContents))]
    internal static class YayoCombatMod_EnemyAmmoLimitPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var replaced = false;
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int value && value == 500)
                {
                    instruction.operand = int.MaxValue;
                    replaced = true;
                }

                yield return instruction;
            }

            if (!replaced)
                Log.Warning("[YayosCombatAutoReloadFix] 未找到 Yayo enemyAmmo 的 500% 上限指令。");
        }
    }

    [HarmonyPatch(typeof(yayoCombat.YayoCombatCore), nameof(yayoCombat.YayoCombatCore.ApplySettingsFrom))]
    internal static class YayoCombatCore_EnemyAmmoLimitPatch
    {
        [HarmonyPostfix]
        private static void Postfix(yayoCombat.YayoCombatSettings settings)
        {
            Apply(settings);
        }

        internal static void ApplyCurrentSetting()
        {
            Apply(yayoCombat.YayoCombatMod.Instance?.Settings);
        }

        private static void Apply(yayoCombat.YayoCombatSettings settings)
        {
            if (settings == null)
                return;

            yayoCombat.YayoCombatCore.s_enemyAmmo = Mathf.Max(0f, settings.enemyAmmo / 100f);
        }
    }
}
