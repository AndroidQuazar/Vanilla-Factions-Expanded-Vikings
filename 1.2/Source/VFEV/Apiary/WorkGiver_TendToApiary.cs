using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace VFEV
{
    class WorkGiver_TendToApiary : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(VFEV_DefOf.VFEV_Apiary);
            }
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
            Apiary tNR_Apiary = t as Apiary;
            if (tNR_Apiary == null || !tNR_Apiary.needTend)
            {
                return false;
            }
            if (t.IsBurning())
            {
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(VFEV_DefOf.VFEV_TendToApiary, t);
        }
    }
}
