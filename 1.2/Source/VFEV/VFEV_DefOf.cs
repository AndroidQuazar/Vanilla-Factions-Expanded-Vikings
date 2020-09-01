using System;
using Verse;
using RimWorld;
using Verse.AI;

namespace VFEV
{
    [DefOf]
    class VFEV_DefOf
    {
    #pragma warning disable 0649
        // Beast hunt
        public static PawnKindDef VFEV_Fenrir;
        public static PawnKindDef VFEV_Lothurr;
        public static PawnKindDef VFEV_Njorun;
        public static PawnKindDef VFEV_Mech_Odin;

        public static ThoughtDef VFEV_BeastHunted;

        // Apiary
        public static ThingDef VFEV_Apiary;
        public static JobDef VFEV_TakeHoneyOutOfApiary;
        public static JobDef VFEV_TendToApiary;
        public static HediffDef VFEV_Sting;
        public static ThoughtDef VFEV_StingMoodDebuff;
        public static ThingDef VFEV_Honey;
        public static DamageDef VFEV_DamageSting;

        // Orbital lightning strike
        public static ThingDef VFEV_LightningStrike;

        public static JobDef VFEV_HypothermiaResponse;

        public static JobDef VFEV_ChangeFacepaint;

        // raids
        public static ThingDef VFEV_Apparel_TorchBelt;
        public static FactionDef VFEV_VikingsSlaver;
        public static FactionDef VFEV_VikingsClan;
        public static DutyDef VFEV_BurnAndStealColony;
        public static JobDef VFEV_IgniteWithTorches;

        //feast
        public static DutyDef VFEV_Feast;
        public static InteractionDef VFEV_DrunkChitchat;
        public static InteractionDef VFEV_VeryDrunkChitchat;
        public static ThoughtDef VFEV_AttendedFeast;

        //mental break
        public static MentalBreakDef VFEV_Enraged;

    }
}
