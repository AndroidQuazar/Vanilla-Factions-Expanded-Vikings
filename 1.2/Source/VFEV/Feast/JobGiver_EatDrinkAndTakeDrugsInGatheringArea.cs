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
            try
            {
                PawnDuty duty = pawn.mindState?.duty;
                if (duty == null)
                {
                    //Log.Message(pawn + " - TryGiveJob - return null; - 4", true);
                    return null;
                }
                if (pawn.needs?.food == null && (double)pawn.needs.food.CurLevelPercentage > 0.9)
                {
                    //Log.Message(pawn + " - TryGiveJob - return null; - 6", true);
                    return null;
                }
                IntVec3 cell = IntVec3.Invalid;
                if (duty.focus.IsValid)
                {
                    cell = duty.focus.Cell;
                }
                else
                {
                    cell = pawn.Position;
                }
                Thing thing = FindFood(pawn, cell);
                if (thing == null)
                {
                    //Log.Message(pawn + " - TryGiveJob - return null; - 10", true);
                    return null;
                }
                Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                job.count = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.def.GetStatValueAbstract(StatDefOf.Nutrition));
                //Log.Message(pawn + " - TryGiveJob - return job; - " + job, true);
                return job;
            }
            catch (Exception ex)
            {
                Log.Error("Excention in Feast: " + ex, true);
                return null;
            }
		}

        private static Thing SpawnedFoodSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, 
            float maxDistance = 9999f, Predicate<Thing> validator = null)
        {
            if (searchSet == null)
            {
                return null;
            }
            Pawn pawn = traverseParams.pawn ?? eater;
            int num = 0;
            int num2 = 0;
            Thing result = null;
            float num3 = 0f;
            float num4 = float.MinValue;
            for (int i = 0; i < searchSet.Count; i++)
            {
                Thing thing = searchSet[i];
                num2++;
                float num5 = (root - thing.Position).LengthManhattan;
                if (!(num5 > maxDistance))
                {
                    num3 = FoodUtility.FoodOptimality(eater, thing, FoodUtility.GetFinalIngestibleDef(thing), num5);
                    if (!(num3 < num4) && thing.Spawned && (validator == null || validator(thing)))
                    {
                        result = thing;
                        num4 = num3;
                        num++;
                    }
                }
            }
            return result;
        }
        private Thing FindFood(Pawn pawn, IntVec3 gatheringSpot)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                //Log.Message("Item candidate: " + x, true);
                if (IntVec3Utility.DistanceTo(x.Position, pawn.Position) > 50f)
                {
                    //Log.Message(" - FindFood - return false; - 1", true);
                    return false;
                }
                if (!x.IngestibleNow)
                {
                    //Log.Message(" - FindFood - return false; - 2", true);
                    return false;
                }
                if (!x.def.IsNutritionGivingIngestible)
                {
                    //Log.Message(" - FindFood - return false; - 4", true);
                    return false;
                }
                if (x.def.IsDrug && !pawn.drugs.CurrentPolicy[x.def].allowedForJoy)
                {
                    //Log.Message(" - FindFood - return false; - 8", true);
                    return false;
                }
                if ((int)x.def.ingestible.preferability <= 4 && !x.def.IsDrug)
                {
                    //Log.Message(x.def + " - " + x.def.ingestible.preferability, true);
                    //Log.Message(" - FindFood - return false; - 10", true);
                    return false;
                }
                if (!x.def.IsDrug && !pawn.WillEat(x))
                {
                    //Log.Message(" - FindFood - return false; - 12", true);
                    return false;
                }
                if (x.IsForbidden(pawn))
                {
                    //Log.Message(" - FindFood - return false; - 14", true);
                    return false;
                }
                if (!x.IsSociallyProper(pawn))
                {
                    //Log.Message(" - FindFood - return false; - 16", true);
                    return false;
                }
                //Log.Message(pawn + " canReserve " + x + ": " + canReserve, true);
                return !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly);
            };

            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
            Thing foodCandidate = null;
            if (pawn.needs.food.CurLevel < 0.4)
            {
                foodCandidate = SpawnedFoodSearchInnerScan(pawn, pawn.Position, list, PathEndMode.OnCell, TraverseParms.For(TraverseMode.ByPawn, Danger.Deadly), 50, validator);
            }
            else
            {
                foodCandidate = list.Where(x => validator(x)).OrderByDescending(x => x.def.ingestible?.joy).ToList().FirstOrDefault();
                if (foodCandidate is null || !(foodCandidate.def.ingestible?.joy > 0))
                {
                    foodCandidate = SpawnedFoodSearchInnerScan(pawn, pawn.Position, list, PathEndMode.OnCell, TraverseParms.For(TraverseMode.ByPawn, Danger.Deadly), 50, validator);
                }
            }
            //Log.Message(pawn + " - " + foodCandidate, true);
            return foodCandidate;
        }
	}
}
