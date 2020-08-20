using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VFEV
{
	public class JobGiver_EatDrinkAndTakeDrugsInGatheringArea : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Log.Message(pawn + " at feast", true);
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				Log.Message(pawn + " - TryGiveJob - return null; - 4", true);
				return null;
			}
			if ((double)pawn.needs.food.CurLevelPercentage > 0.9)
			{
				Log.Message(pawn + " - TryGiveJob - return null; - 6", true);
				return null;
			}
			IntVec3 cell = duty.focus.Cell;
			Thing thing = FindFood(pawn, cell);
			if (thing == null)
			{
				Log.Message(pawn + " - TryGiveJob - return null; - 10", true);
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
			job.count = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.def.GetStatValueAbstract(StatDefOf.Nutrition));
			Log.Message(pawn + " - TryGiveJob - return job; - " + job, true);
			return job;
		}



        private Thing FindFood(Pawn pawn, IntVec3 gatheringSpot)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                Log.Message("Item candidate: " + x, true);
                if (!x.IngestibleNow)
                {
                    Log.Message(" - FindFood - return false; - 2", true);
                    return false;
                }
                if (!x.def.IsNutritionGivingIngestible)
                {
                    Log.Message(" - FindFood - return false; - 4", true);
                    return false;
                }
                if (x.def.IsDrug && !pawn.drugs.CurrentPolicy[x.def].allowedForJoy)
                {
                    Log.Message(" - FindFood - return false; - 8", true);
                    return false;
                }
                if ((int)x.def.ingestible.preferability <= 4 && !x.def.IsDrug)
                {
                    Log.Message(x.def + " - " + x.def.ingestible.preferability, true);
                    Log.Message(" - FindFood - return false; - 10", true);
                    return false;
                }
                if (!pawn.WillEat(x))
                {
                    Log.Message(" - FindFood - return false; - 12", true);
                    return false;
                }
                if (x.IsForbidden(pawn))
                {
                    Log.Message(" - FindFood - return false; - 14", true);
                    return false;
                }
                if (!x.IsSociallyProper(pawn))
                {
                    Log.Message(" - FindFood - return false; - 16", true);
                    return false;
                }
                return pawn.CanReserve(x) ? true : false;
            };
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree).OrderByDescending(x => x.def.ingestible.joy).ToList();
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, list, PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors), 50, validator);
        }
	}
}
