using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEV
{
    public class JoyGiver_PlayTrainingDummy : JoyGiver_WatchBuilding
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            return this.TryGiveJob(pawn, null, false);
        }

        public Job TryGiveJob(Pawn pawn, Thing targetThing, bool NoJoyCheck = false)
        {
            Verb verb = null;
            if (pawn != null)
            {
                verb = pawn.meleeVerbs.TryGetMeleeVerb(targetThing);
            }
            Job result;
            if (pawn.WorkTagIsDisabled(WorkTags.Violent) || verb == null || verb.verbProps == null)
            {
                result = null;
            }
            else
            {
                List<Thing> list = pawn.Map.listerThings.ThingsOfDef(this.def.thingDefs[0]);
                Predicate<Thing> predicate = delegate (Thing t)
                {
                    return !ForbidUtility.IsForbidden(t, pawn)
                    && ReservationUtility.CanReserve(pawn, t, this.def.jobDef.joyMaxParticipants, -1, null, false)
                    && SocialProperness.IsSociallyProper(t, pawn);
                };
                Thing thing = null;
                if (targetThing != null && ReachabilityUtility.CanReach(pawn, targetThing.Position, PathEndMode.InteractionCell, Danger.Deadly, false, 0) && predicate(targetThing))
                {
                    thing = targetThing;
                }
                else if (targetThing == null)
                {
                    thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, list, PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Deadly, 0, false), 9999f, predicate, null);
                }
                if (thing != null)
                {
                    Job job = JobMaker.MakeJob(this.def.jobDef, thing);
                    return job;
                }
                result = null;
            }
            return result;
        }
    }
}