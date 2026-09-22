using HarmonyLib;
using RimWorld;
using Verse;
using YayosCombatAddon;

namespace YayosCombatAutoReloadFix
{
    [HarmonyPatch(typeof(Pawn), "Tick")]
    internal static class NpcEmptyWeaponRetryPatch
    {
        private const int MinimumIntervalTicks = 120;
        private const uint IntervalVariationTicks = 61;

        [HarmonyPostfix]
        private static void Postfix(Pawn __instance)
        {
            if (!AutoReloadFixMod.CurrentSettings.EnableNpcEmptyWeaponRetry
                || !yayoCombat.YayoCombatCore.ammo
                || (__instance.Faction == Faction.OfPlayer && __instance.RaceProps.Humanlike)
                || !__instance.Spawned
                || __instance.equipment == null
                || __instance.inventory == null)
            {
                return;
            }

            var intervalTicks = MinimumIntervalTicks
                + (int)((uint)__instance.thingIDNumber % IntervalVariationTicks);
            if (!__instance.IsHashIntervalTick(intervalTicks)
                || !__instance.IsCapableOfReloading()
                || IsReloadJob(__instance.CurJobDef))
            {
                return;
            }

            var primary = __instance.equipment.Primary;
            var comp = primary?.TryGetComp<CompApparelReloadable>();
            if (comp == null || !AmmoUtility.IsAmmo(comp.AmmoDef))
            {
                return;
            }

            var retryingPartialReload = ReloadRetryRegistry.IsPending(__instance, primary)
                && comp.RemainingCharges < comp.MaxCharges;
            if (comp.RemainingCharges > 0 && !retryingPartialReload)
                return;

            if (__instance.CountAmmoInInventory(comp) >= comp.MinAmmoNeededChecked())
            {
                if (ReloadUtility.TryReloadFromInventory(__instance, new[] { primary }, false))
                    ReloadRetryRegistry.Clear(__instance, primary);
                return;
            }

            VirtualRefillUtility.TryStart(comp);
        }

        private static bool IsReloadJob(JobDef jobDef)
        {
            var defName = jobDef?.defName;
            return defName == "YCA_ReloadFromInventory"
                || defName == "YCA_ReloadFromSurrounding"
                || defName == "Reload"
                || VirtualRefillUtility.IsVirtualReloadJob(jobDef);
        }
    }
}
