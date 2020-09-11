using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VFEV
{
	public class GatheringWorker_Feast : GatheringWorker
	{
		protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
		{
			return new LordJob_Joinable_Feast(spot, organizer, DefDatabase<GatheringDef>.GetNamed("VFEV_Feast"));
		}

        public override bool CanExecute(Map map, Pawn organizer = null)
        {
			var nextTick = new IntRange(3500000, 3700000).RandomInRange;
		 //Log.Message("map.GetComponent<VFEV_MapComponentHelper>().lastFeastStartTick: " + map.GetComponent<VFEV_MapComponentHelper>().lastFeastStartTick, true);
		 //Log.Message("nextTick: " + nextTick, true);
		 //Log.Message("Find.TickManager.TicksGame: " + Find.TickManager.TicksGame, true);

			if (map.GetComponent<VFEV_MapComponentHelper>().lastFeastStartTick + nextTick > Find.TickManager.TicksGame)
            {
			 //Log.Message("Result false");
				return false;
            }
			if (organizer == null)
			{
				organizer = FindOrganizer(map);
			}
			if (organizer == null)
			{
				return false;
			}
			if (!TryFindGatherSpot(organizer, out IntVec3 _))
			{
				return false;
			}
			if (!GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
			{
				return false;
			}
			return true;
		}

        public override bool TryExecute(Map map, Pawn organizer = null)
        {
            var result = base.TryExecute(map, organizer);
			if (result)
            {
				map.GetComponent<VFEV_MapComponentHelper>().lastFeastStartTick = Find.TickManager.TicksGame;
            }
			return result;
		}
        protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
		{
			return RCellFinder.TryFindGatheringSpot_NewTemp(organizer, DefDatabase<GatheringDef>.GetNamed("VFEV_Feast"), ignoreRequiredColonistCount: false, out spot);
		}
	}
}
