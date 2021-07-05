using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEV
{
	public class JobGiver_CaptureDownedVictimAndLeaveMap : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.mindState.meleeThreat != null && pawn.mindState.meleeThreat.RaceProps.Humanlike 
				&& pawn.mindState.meleeThreat.Downed && pawn.mindState.meleeThreat.HostileTo(pawn))
			{
				if (ReservationUtility.CanReserve(pawn, pawn.mindState.meleeThreat))
				{
					if (!RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
					{
						return null;
					}
					//Log.Message(pawn + " - meleeThreat: " + pawn.mindState.meleeThreat);
					Job job = JobMaker.MakeJob(JobDefOf.Kidnap);
					job.targetA = pawn.mindState.meleeThreat;
					job.targetB = spot;
					job.count = 1;
					return job;
				}
			}
			else if (pawn.mindState.enemyTarget != null && pawn.mindState.enemyTarget is Pawn pawn2 
				&& pawn2.RaceProps.Humanlike && pawn2.Downed && pawn2.HostileTo(pawn))
			{
				if (ReservationUtility.CanReserve(pawn, pawn.mindState.enemyTarget))
				{
					if (!RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
					{
						return null;
					}
					//Log.Message(pawn + " - enemyTarget: " + pawn2);
					Job job = JobMaker.MakeJob(JobDefOf.Kidnap);
					job.targetA = pawn.mindState.enemyTarget;
					job.targetB = spot;
					job.count = 1;
					return job;
				}
			}
			else
			{
				var victim = pawn.Map.mapPawns.AllPawns.Where(x => x.RaceProps.Humanlike 
				&& x.HostileTo(pawn) && x.Downed
				&& x.Position.DistanceTo(pawn.Position) < 15).FirstOrDefault();
				if (victim != null && ReservationUtility.CanReserve(pawn, victim))
				{
					if (!RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
					{
						return null;
					}
					//Log.Message(pawn + " - victim: " + victim);
					Job job = JobMaker.MakeJob(JobDefOf.Kidnap);
					job.targetA = victim;
					job.targetB = spot;
					job.count = 1;
					return job;
				}
			}
			return null;
		}


	}
}
