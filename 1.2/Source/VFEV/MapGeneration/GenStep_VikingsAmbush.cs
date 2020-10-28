using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VFEV
{
    class GenStep_VikingsAmbush : GenStep
    {
		public override int SeedPart
		{
			get
			{
				return 936817770;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, map);
			incidentParms.faction = parms.sitePart.site.Faction;
			if (incidentParms.faction == null) incidentParms.faction = Faction.OfMechanoids;
			incidentParms.points = parms.sitePart.parms.threatPoints;
			
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, false), true).ToList<Pawn>();
			if (list.Count > 0)
			{
				foreach (Pawn pawn in list)
				{
					IntVec3 result = new IntVec3();
					CellFinder.TryFindRandomCellNear(map.Center, map, 10, c => c.Walkable(map), out result);
					GenSpawn.Spawn(pawn, result, map);
				}
			}
			MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(map.Center, 1, 1));
			LordMaker.MakeNewLord(incidentParms.faction, new LordJob_DefendBase(incidentParms.faction, map.Center), map, list);
		}
	}
}
