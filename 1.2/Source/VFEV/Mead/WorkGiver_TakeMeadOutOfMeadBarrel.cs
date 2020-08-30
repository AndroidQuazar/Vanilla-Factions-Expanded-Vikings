using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;

namespace VFEV
{
    public class WorkGiver_TakeMeadOutOfMeadBarrel : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {

            return pawn.Map.GetComponent<MeadBarrels_MapComponent>().meadBarrels_InMap;


        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Thing> list = pawn.Map.GetComponent<MeadBarrels_MapComponent>().meadBarrels_InMap.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (((Building_MeadBarrel)list[i]).Fermented)
                {
                    return false;
                }
            }
            return true;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_MeadBarrel building_FermentingBarrel = t as Building_MeadBarrel;
            return building_FermentingBarrel != null && building_FermentingBarrel.Fermented && !t.IsBurning() && !t.IsForbidden(pawn) && pawn.CanReserve(t, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VFEV_TakeMeadOutOfMeadBarrel", true), t);
        }
    }
}
