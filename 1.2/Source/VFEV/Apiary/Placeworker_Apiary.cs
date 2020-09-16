using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEV
{
    class Placeworker_Apiary : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			/*foreach (Thing apiary in map.listerBuildings.allBuildingsColonist.FindAll(b => b.def.defName == "VFEV_Apiary"))
            {
				if (apiary != null && apiary.CellsAdjacent8WayAndInside().Contains(center))
                {
					return "APlaceWorker".Translate();
				}
            }*/
			CellRect cellAround = CellRect.CenteredOn(center, 1);
            foreach (IntVec3 cell in cellAround)
            {
				if (cell.GetFirstBuilding(map)?.def?.defName == "VFEV_Apiary") return "APlaceWorker".Translate();
			}
			if (center.Roofed(map)) return "APlaceWorkerNoRoof".Translate();
			return true;
		}
	}
}
