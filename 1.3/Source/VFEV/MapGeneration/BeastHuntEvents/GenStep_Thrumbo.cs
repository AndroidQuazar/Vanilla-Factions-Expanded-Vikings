using System;
using RimWorld.Planet;
using Verse;
using RimWorld;
using Verse.AI.Group;
using System.Collections.Generic;

namespace VFEV.MapGeneration.BeastHuntEvents
{
    class GenStep_Thrumbo : GenStep_Scatterer
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
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Thrumbo);
			GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false);
			MapGenerator.rootsToUnfog.Add(loc);
			MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
		}
	}
}
