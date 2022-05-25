using RimWorld;
using UnityEngine;
using Verse;


namespace VFEV
{




    public class VFEV_Mod : Mod
    {
        public static VFEV_Settings settings;

        public VFEV_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<VFEV_Settings>();
        }
        public override string SettingsCategory() => "Vanilla Factions Expanded - Vikings";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }





    }
}

