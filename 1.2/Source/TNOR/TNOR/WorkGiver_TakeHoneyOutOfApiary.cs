using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace TNOR
{
    class WorkGiver_TakeHoneyOutOfApiary : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(TNOR_DefOf.TNR_Apiary);
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
            TNR_Apiary tNR_Apiary = t as TNR_Apiary;
            int skill = pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").levelInt;
            if (tNR_Apiary == null || !tNR_Apiary.HoneyReady || skill < 5)
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
            return new Job(TNOR_DefOf.TNR_TakeHoneyOutOfApiary, t);
        }
    }
}
