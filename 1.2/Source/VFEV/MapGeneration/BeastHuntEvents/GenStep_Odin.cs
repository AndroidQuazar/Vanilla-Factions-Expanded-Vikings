using System;
using RimWorld.Planet;
using Verse;
using RimWorld;
using Verse.AI.Group;
using System.Collections.Generic;

namespace VFEV.MapGeneration.BeastHuntEvents
{
    class GenStep_Odin : GenStep_Scatterer
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
			Pawn pawn = PawnGenerator.GeneratePawn(VFEV_DefOf.VFEV_Mech_Odin, Faction.OfMechanoids);

			List<Pawn> pawns = new List<Pawn>();
			var lord = new LordJob_AssaultColony(Faction.OfMechanoids, false, true, true, true, true);
			GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
			pawns.Add(pawn);
			LordMaker.MakeNewLord(Faction.OfMechanoids, lord, map, pawns);
			
			
			MapGenerator.rootsToUnfog.Add(loc);
			MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
		}
	}
}
