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
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
			incidentParms.faction = Find.FactionManager.AllFactionsListForReading.Find(f => !f.defeated && f.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile && f.def.defName.Contains("VFEV_Vikings"));
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

			// MapGenerator.rootsToUnfog.Add(map.Center);
			MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(map.Center, 1, 1));

			LordMaker.MakeNewLord(incidentParms.faction, new LordJob_DefendBase(incidentParms.faction, map.Center), map, list);
		}
	}
}
