using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace VFEV
{
    public class WorkGiver_FillMeadBarrel : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {

            return pawn.Map.GetComponent<MeadBarrels_MapComponent>().meadBarrels_InMap;


        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public static void ResetStaticData()
        {
            WorkGiver_FillMeadBarrel.TemperatureTrans = "BadTemperature".Translate().ToLower();
            WorkGiver_FillMeadBarrel.NoWortTrans = "VFEV_NoHoney".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_MeadBarrel building_FermentingBarrel = t as Building_MeadBarrel;
            if (building_FermentingBarrel == null || building_FermentingBarrel.Fermented || building_FermentingBarrel.SpaceLeftForWort <= 0)
            {
                return false;
            }
            float ambientTemperature = building_FermentingBarrel.AmbientTemperature;
            CompProperties_TemperatureRuinable compProperties = building_FermentingBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(WorkGiver_FillMeadBarrel.TemperatureTrans, null);
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (this.FindWort(pawn, building_FermentingBarrel) == null)
            {
                JobFailReason.Is(WorkGiver_FillMeadBarrel.NoWortTrans, null);
                return false;
            }
            return !t.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_MeadBarrel barrel = (Building_MeadBarrel)t;
            Thing t2 = this.FindWort(pawn, barrel);
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VFEV_FillMeadBarrel", true), t, t2);
        }

        private Thing FindWort(Pawn pawn, Building_MeadBarrel barrel)
        {
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDef.Named("VFEV_Honey")), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        private static string TemperatureTrans;

        private static string NoWortTrans;
    }
}
