using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEV
{
    class ThinkNode_HypothermiaResponse : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return (pawn.IsColonistPlayerControlled && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving)) 
                && !pawn.Downed 
                && !pawn.IsBurning() 
                && !pawn.InMentalState 
                && !pawn.Drafted 
                && pawn.Awake() 
                && !HealthAIUtility.ShouldSeekMedicalRest(pawn);
        }
    }

    class JobDriver_HypothermiaResponse : JobDriver
    {
		public Building_Bed Bed
		{
			get
			{
				return (Building_Bed)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (this.job.GetTarget(TargetIndex.A).HasThing && this.job.GetTarget(TargetIndex.A).Thing.def.defName == "")
			{
				Pawn pawn = this.pawn;
				LocalTargetInfo target = this.Bed;
				Job job = this.job;
				int sleepingSlotsCount = this.Bed.SleepingSlotsCount;
				int stackCount = 0;
				if (!pawn.Reserve(target, job, sleepingSlotsCount, stackCount, null, errorOnFailed))
				{
					return false;
				}
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return JobInBedUtility.InBedOrRestSpotNow(this.pawn, this.job.GetTarget(TargetIndex.A));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool hasBed = this.job.GetTarget(TargetIndex.A).HasThing;
			if (hasBed)
			{
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
				yield return Toils_Bed.GotoBed(TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			}
			yield return Toil_HypothermiaResponse.LayDown(TargetIndex.A, hasBed, false, true, true);
			yield break;
		}

		public override string GetReport()
		{
			return "Recorvering from hypothermia";
		}

		public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;
	}

	class JobGiver_HypothermiaResponse : ThinkNode_JobGiver
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.InMentalState)
			{
				return null;
			}
			if (pawn.Map == null)
			{
				return null;
			}
			if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
            {
				return null;
            }
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return null;
			}
			if (pawn.CurJobDef != null && pawn.CurJobDef == VFEV_DefOf.VFEV_HypothermiaResponse)
			{
				return null;
			}
			if (!pawn.health.hediffSet.HasHediff(HediffDefOf.Hypothermia, false))
			{
				return null;
			}
			if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia, false) is Hediff hediffHypothermia && hediffHypothermia != null && hediffHypothermia.Severity >= 0.35f)
			{
				Building_Bed bed = this.FindFurBedFor(pawn, pawn, false, true);
				if (bed != null) return new Job(VFEV_DefOf.VFEV_HypothermiaResponse, bed);
			}
			return null;
		}

		private Building_Bed FindFurBedFor(Pawn sleeper, Pawn traveler, bool sleeperWillBePrisoner, bool checkSocialProperness, bool ignoreOtherReservations = false)
		{
			if (sleeper.ownership != null && sleeper.ownership.OwnedBed != null && (sleeper.ownership.OwnedBed.def.defName == "VFEV_FurBed" || sleeper.ownership.OwnedBed.def.defName == "VFEV_DoubleFurBed") 
				&& RestUtility.IsValidBedFor(sleeper.ownership.OwnedBed, sleeper, traveler, sleeperWillBePrisoner, checkSocialProperness, false, ignoreOtherReservations))
			{
				return sleeper.ownership.OwnedBed;
			}
			return null;
		}
	}

	public static class Toil_HypothermiaResponse
    {
		public static Toil LayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs, bool canSleep = true, bool gainRestAndHealth = true)
		{
			Toil layDown = new Toil();
			layDown.initAction = delegate ()
			{
				Pawn actor = layDown.actor;
				actor.pather.StopDead();
				JobDriver curDriver = actor.jobs.curDriver;
				if (hasBed)
				{
					if (!((Building_Bed)actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing).OccupiedRect().Contains(actor.Position))
					{
						Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor, false);
						actor.jobs.EndCurrentJob(JobCondition.Errored, true, true);
						return;
					}
					actor.jobs.posture = PawnPosture.LayingInBed;
				}
				else
				{
					actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
				}
				curDriver.asleep = false;
				if (actor.mindState.applyBedThoughtsTick == 0)
				{
					actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
					actor.mindState.applyBedThoughtsOnLeave = false;
				}
				if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
				{
					ThoughtUtility.RemovePositiveBedroomThoughts(actor);
				}
				CompCanBeDormant comp = actor.GetComp<CompCanBeDormant>();
				if (comp != null)
				{
					comp.ToSleep();
				}
			};
			layDown.tickAction = delegate ()
			{
				Pawn actor = layDown.actor;
				Job curJob = actor.CurJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Building_Bed building_Bed = (Building_Bed)curJob.GetTarget(bedOrRestSpotIndex).Thing;
				actor.GainComfortFromCellIfPossible(false);
				if (!curDriver.asleep)
				{
					if (canSleep && ((actor.needs.rest != null && actor.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor)) || curJob.forceSleep))
					{
						curDriver.asleep = true;
					}
				}
				else if (!canSleep)
				{
					curDriver.asleep = false;
				}
				else if ((actor.needs.rest == null || actor.needs.rest.CurLevel >= RestUtility.WakeThreshold(actor)) && !curJob.forceSleep)
				{
					curDriver.asleep = false;
				}
				if (curDriver.asleep && gainRestAndHealth && actor.needs.rest != null)
				{
					float restEffectiveness;
					if (building_Bed != null && building_Bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
					{
						restEffectiveness = building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness, true);
					}
					else
					{
						restEffectiveness = StatDefOf.BedRestEffectiveness.valueIfMissing;
					}
					actor.needs.rest.TickResting(restEffectiveness);
				}
				if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
				{
					if (curDriver.asleep)
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_SleepZ);
					}
					if (gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_HealingCross);
					}
				}
				if (actor.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.OwnersForReading.Contains(actor))
				{
					if (actor.Downed)
					{
						actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
					return;
				}
				if (lookForOtherJobs && actor.IsHashIntervalTick(211))
				{
					actor.jobs.CheckForJobOverride();
					return;
				}
			};
			layDown.AddEndCondition(delegate ()
			{
				Pawn actor = layDown.actor;
				if (!actor.health.hediffSet.HasHediff(HediffDefOf.Hypothermia))
				{
					return JobCondition.Succeeded;
				}
				else return JobCondition.Ongoing;
			});
			layDown.defaultCompleteMode = ToilCompleteMode.Never;
			if (hasBed)
			{
				layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
			}
			layDown.AddFinishAction(delegate
			{
				Pawn actor = layDown.actor;
				JobDriver curDriver = actor.jobs.curDriver;

				curDriver.asleep = false;
			});
			return layDown;
		}

		private const int TicksBetweenSleepZs = 100;
		private const int GetUpOrStartJobWhileInBedCheckInterval = 211;
	}
}
