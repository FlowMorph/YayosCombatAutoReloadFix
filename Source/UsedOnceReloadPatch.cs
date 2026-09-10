using HarmonyLib;
using RimWorld;
using Verse;
using YayosCombatAddon;

namespace YayosCombatAutoReloadFix
{
    [StaticConstructorOnStartup]
    internal static class PatchBootstrap
    {
        static PatchBootstrap()
        {
            new Harmony("local.YayosCombatAutoReloadFix").PatchAll();
        }
    }

    [HarmonyPatch(typeof(CompApparelReloadable), nameof(CompApparelReloadable.UsedOnce))]
    internal static class UsedOnceReloadPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CompApparelReloadable __instance)
        {
            if (!yayoCombat.YayoCombatCore.ammo || __instance.Wearer == null)
            {
                return;
            }

            if (__instance.ReloadableThing?.def?.equipmentType != EquipmentType.Primary)
            {
                return;
            }

            if (!AmmoUtility.IsAmmo(__instance.AmmoDef))
            {
                return;
            }

            var pawn = __instance.Wearer;
            var drafted = pawn.Drafted;
            if (!ReloadUtility.TryAutoReloadSingle(
                    __instance,
                    showOutOfAmmoWarning: true,
                    ignoreDistance: !drafted,
                    returnToStartingPosition: drafted)
                && pawn.CurJobDef == JobDefOf.Hunt)
            {
                pawn.jobs.StopAll();
            }
        }
    }
}
