using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using YayosCombatAddon;

namespace YayosCombatAutoReloadFix
{
    internal static class VirtualRefillJobDefOf
    {
        internal static JobDef VirtualRefill { get; private set; }

        internal static void Initialize()
        {
            VirtualRefill = DefDatabase<JobDef>.GetNamedSilentFail("YCA_VirtualRefill")
                ?? new JobDef
                {
                    defName = "YCA_VirtualRefill",
                    driverClass = typeof(JobDriver_VirtualRefill),
                    reportString = "Reloading",
                    playerInterruptible = true,
                    suspendable = false,
                    allowOpportunisticPrefix = false
                };

            if (!DefDatabase<JobDef>.AllDefsListForReading.Contains(VirtualRefill))
                DefDatabase<JobDef>.Add(VirtualRefill);
        }
    }

    internal sealed class JobDriver_VirtualRefill : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => pawn == null);
            this.FailOn(() => pawn.Downed);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            yield return Toils_General.Wait(GetReloadTicks())
                .WithProgressBarToilDelay(TargetIndex.A);

            yield return new Toil
            {
                initAction = () => VirtualRefillUtility.Complete(pawn, TargetThingA)
            };
        }

        private int GetReloadTicks()
        {
            var comp = TargetThingA?.TryGetComp<CompApparelReloadable>();
            return comp == null ? 1 : UnityEngine.Mathf.Max(1, comp.Props.baseReloadTicks);
        }
    }

    internal static class VirtualRefillUtility
    {
        internal static bool TryHandleSingle(CompApparelReloadable comp, ref bool result)
        {
            if (!IsVirtualRefillCase(comp))
                return false;

            result = TryStart(comp);
            return true;
        }

        internal static bool TryHandleAll(Pawn pawn, ref bool result)
        {
            if (pawn == null)
                return false;

            var virtualComp = pawn.GetAllReloadableThings()
                .Select(thing => thing.TryGetComp<CompApparelReloadable>())
                .FirstOrDefault(IsVirtualRefillCase);
            if (virtualComp == null)
                return false;

            result = TryStart(virtualComp);
            return true;
        }

        internal static bool TryStart(CompApparelReloadable comp)
        {
            if (!IsVirtualRefillCase(comp))
                return false;

            var pawn = comp.Wearer as Pawn;
            if (pawn == null || IsVirtualReloadJob(pawn.CurJobDef))
                return false;

            ReloadRetryRegistry.Clear(pawn, comp.parent);
            var job = JobMaker.MakeJob(VirtualRefillJobDefOf.VirtualRefill, comp.parent);
            pawn.jobs.StartJob(
                job,
                JobCondition.InterruptForced,
                resumeCurJobAfterwards: true,
                canReturnCurJobToPool: true);
            return true;
        }

        internal static void Complete(Pawn pawn, Thing weapon)
        {
            var comp = weapon?.TryGetComp<CompApparelReloadable>();
            if (pawn == null
                || !IsVirtualRefillCase(comp)
                || comp.RemainingCharges >= comp.MaxCharges)
            {
                return;
            }

            ConsumeAvailableAmmo(pawn, comp);
            Traverse.Create(comp).Field("remainingCharges").SetValue(comp.MaxCharges);
            ReloadRetryRegistry.Clear(pawn, weapon);
        }

        private static void ConsumeAvailableAmmo(Pawn pawn, CompApparelReloadable comp)
        {
            var remaining = comp.MaxAmmoNeeded(false);
            if (remaining <= 0)
                return;

            foreach (var ammo in pawn.inventory.innerContainer.ToList())
            {
                if (remaining <= 0 || ammo.def != comp.AmmoDef)
                    continue;

                var count = Math.Min(remaining, ammo.stackCount);
                if (count == ammo.stackCount)
                    ammo.Destroy();
                else
                    ammo.stackCount -= count;
                remaining -= count;
            }
        }

        internal static bool IsVirtualRefillCase(CompApparelReloadable comp)
        {
            var pawn = comp?.Wearer as Pawn;
            return comp != null
                && pawn != null
                && comp.RemainingCharges < comp.MaxCharges
                && !pawn.RaceProps.Humanlike
                && yayoCombat.YayoCombatCore.refillMechAmmo
                && AmmoUtility.IsAmmo(comp.AmmoDef)
                && pawn.IsCapableOfReloading()
                && (comp.RemainingCharges <= 0 || ReloadRetryRegistry.IsPending(pawn, comp.parent))
                && pawn.CountAmmoInInventory(comp) < comp.MinAmmoNeededChecked();
        }

        internal static bool IsVirtualReloadJob(JobDef jobDef) =>
            jobDef == VirtualRefillJobDefOf.VirtualRefill;
    }

    [HarmonyPatch(typeof(ReloadUtility), nameof(ReloadUtility.TryAutoReloadSingle))]
    internal static class ReloadUtility_TryAutoReloadSingle_VirtualRefillPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(CompApparelReloadable comp, ref bool __result) =>
            !VirtualRefillUtility.TryHandleSingle(comp, ref __result);
    }

    [HarmonyPatch(typeof(ReloadUtility), nameof(ReloadUtility.TryAutoReloadAll))]
    internal static class ReloadUtility_TryAutoReloadAll_VirtualRefillPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, ref bool __result) =>
            !VirtualRefillUtility.TryHandleAll(pawn, ref __result);
    }
}
