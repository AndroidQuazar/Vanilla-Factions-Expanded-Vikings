using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEV
{
    class GenStep_VikingsAmbush : GenStep_Scatterer
    {
		public override int SeedPart
		{
			get
			{
				return 936817770;
			}
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return base.CanScatterAt(c, map) && c.Standable(map);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
			incidentParms.faction = Find.FactionManager.AllFactionsListForReading.Find(f=>!f.defeated && f.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile && f.def.defName.Contains("VFEV_Vikings"));
			incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target);

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

			MapGenerator.rootsToUnfog.Add(map.Center);
			MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(map.Center, 1, 1));
		}
	}
}
