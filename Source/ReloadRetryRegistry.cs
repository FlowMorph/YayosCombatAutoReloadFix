using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using YayosCombatAddon;

namespace YayosCombatAutoReloadFix
{
    internal static class ReloadRetryRegistry
    {
        private static readonly Dictionary<Pawn, Thing> Pending = new Dictionary<Pawn, Thing>();

        internal static void Mark(Pawn pawn, Thing weapon)
        {
            if (pawn != null && weapon != null)
                Pending[pawn] = weapon;
        }

        internal static bool IsPending(Pawn pawn, Thing weapon)
        {
            return pawn != null
                && weapon != null
                && Pending.TryGetValue(pawn, out var pendingWeapon)
                && pendingWeapon == weapon;
        }

        internal static void Clear(Pawn pawn, Thing weapon)
        {
            if (pawn != null
                && Pending.TryGetValue(pawn, out var pendingWeapon)
                && pendingWeapon == weapon)
            {
                Pending.Remove(pawn);
            }
        }

        internal static void ObserveJobEnd(Pawn pawn, Job job, JobCondition condition)
        {
            if (pawn == null || job == null || !IsReloadJob(job.def))
                return;

            var weapon = job.targetA.Thing;
            var comp = weapon?.TryGetComp<CompApparelReloadable>();
            if (comp == null)
                return;

            if (condition != JobCondition.Succeeded && comp.RemainingCharges < comp.MaxCharges)
                Mark(pawn, weapon);
            else if (condition == JobCondition.Succeeded || comp.RemainingCharges >= comp.MaxCharges)
                Clear(pawn, weapon);
        }

        internal static bool IsReloadJob(JobDef jobDef)
        {
            return jobDef?.defName == "YCA_ReloadFromInventory"
                || jobDef?.defName == "YCA_ReloadFromSurrounding"
                || jobDef?.defName == "Reload"
                || VirtualRefillUtility.IsVirtualReloadJob(jobDef);
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
    internal static class PawnJobTracker_EndCurrentJob_ReloadRetryPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Pawn ___pawn, Job ___curJob, JobCondition condition)
        {
            ReloadRetryRegistry.ObserveJobEnd(___pawn, ___curJob, condition);
        }
    }
}
