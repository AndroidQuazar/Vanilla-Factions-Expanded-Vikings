using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
			incidentParms.faction = Find.FactionManager.AllFactionsListForReading.FindAll(f => (f.def == VFEV_DefOf.VFEV_VikingsClan || f.def == VFEV_DefOf.VFEV_VikingsSlaver) && f.HostileTo(Faction.OfPlayer))?.RandomElement();/*parms.sitePart.site.Faction;*/
			if (incidentParms.faction == null) 
				incidentParms.faction = Faction.OfMechanoids;

			incidentParms.points = Mathf.Max(incidentParms.points * 0.5f, incidentParms.faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, false), true).ToList();
			if (list.Count > 0)
			{
				foreach (Pawn pawn in list)
				{
					IntVec3 result = new IntVec3();
					CellFinder.TryFindRandomCellNear(map.Center, map, 10, c => c.Walkable(map), out result);
					GenSpawn.Spawn(pawn, result, map);
				}
			}
			MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(map.Center, 1, 1));

			if (incidentParms.faction == Faction.OfMechanoids)
				LordMaker.MakeNewLord(incidentParms.faction, new LordJob_DefendPoint(map.Center, 10, addFleeToil: false), map, list);
			else
				LordMaker.MakeNewLord(incidentParms.faction, new LordJob_DefendBase(incidentParms.faction, map.Center), map, list);
		}
	}
}
