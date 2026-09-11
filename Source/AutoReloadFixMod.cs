using UnityEngine;
using Verse;

namespace YayosCombatAutoReloadFix
{
    public sealed class AutoReloadFixSettings : ModSettings
    {
        public bool EnableNpcEmptyWeaponRetry = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableNpcEmptyWeaponRetry, "enableNpcEmptyWeaponRetry", true);
        }
    }

    public sealed class AutoReloadFixMod : Mod
    {
        internal static AutoReloadFixSettings CurrentSettings { get; private set; } = new AutoReloadFixSettings();

        public AutoReloadFixMod(ModContentPack content) : base(content)
        {
            CurrentSettings = GetSettings<AutoReloadFixSettings>();
        }

        public override string SettingsCategory()
        {
            return "YCAARF.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "YCAARF.EnableNpcEmptyWeaponRetry".Translate(),
                ref CurrentSettings.EnableNpcEmptyWeaponRetry,
                "YCAARF.EnableNpcEmptyWeaponRetry.Description".Translate());
            listing.End();
        }
    }
}
