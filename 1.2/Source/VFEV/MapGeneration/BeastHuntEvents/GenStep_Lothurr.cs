using System;
using RimWorld.Planet;
using Verse;
using RimWorld;

namespace VFEV.MapGeneration.BeastHuntEvents
{
    class GenStep_Lothurr : GenStep_Scatterer
    {
		public override int SeedPart
		{
			get
			{
				return 931842770;
			}
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return base.CanScatterAt(c, map) && c.Standable(map);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(VFEV_DefOf.VFEV_Lothurr);
			pawn.mindState.exitMapAfterTick = 120000;
			GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false);
			MapGenerator.rootsToUnfog.Add(loc);
			MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
		}
	}
}
