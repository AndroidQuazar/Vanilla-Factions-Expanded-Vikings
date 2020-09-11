using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEV.MapGeneration
{
    class ScenPart_AddSettlements : ScenPart
    {
		public override string Summary(Scenario scen)
		{
			return "ScenPartSettlemntDesc".Translate();
		}

		public override void PostMapGenerate(Map map)
		{
			if (Find.TickManager.TicksGame < 10f)
			{
				IntRange SettlementSizeRange = new IntRange(38, 16);
				int randomInRange = SettlementSizeRange.RandomInRange;
				int randomInRange2 = SettlementSizeRange.RandomInRange;
				IntVec3 c = map.Center;
				c.x -= 30;
				c.y -= 10;
				CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
				rect.ClipInsideMap(map);

				ResolveParams resolveParams = default(ResolveParams);
				resolveParams.rect = rect;
				resolveParams.faction = Find.FactionManager.AllFactions.ToList().Find(f => f.def.techLevel <= TechLevel.Medieval && !f.defeated && !f.Hidden && f.HostileTo(Faction.OfPlayer));
				resolveParams.settlementPawnGroupPoints = 500f;
				BaseGen.globalSettings.map = map;
				BaseGen.globalSettings.minBuildings = 1;
				BaseGen.globalSettings.minBarracks = 1;
				BaseGen.symbolStack.Push("settlement", resolveParams, null);
				if (resolveParams.faction != null && resolveParams.faction == Faction.Empire)
				{
					BaseGen.globalSettings.minThroneRooms = 1;
					BaseGen.globalSettings.minLandingPads = 1;
				}
				BaseGen.Generate();
				if (resolveParams.faction != null && resolveParams.faction == Faction.Empire && BaseGen.globalSettings.landingPadsGenerated == 0)
				{
					CellRect cellRect;
					GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, resolveParams.faction, out cellRect);
				}

				this.UnfogBuildingsInRect(map, rect);
				this.SpawnThingPostBaseGen(ThingDefOf.Gold, 150, map, rect);
				this.SpawnThingPostBaseGen(ThingDefOf.Silver, 300, map, rect);
			}
		}

		private void UnfogBuildingsInRect(Map map, CellRect rect)
        {
			foreach (IntVec3 i in rect.ExpandedBy(5))
			{
				if (i.Fogged(map) && (i.GetThingList(map).Any((t) => t.Faction != null) || i.Walkable(map))) map.fogGrid.Unfog(i);
			}
		}

		private void SpawnThingPostBaseGen(ThingDef thingDef, int stackCount, Map map, CellRect rect)
        {
			IntVec3 spawnPos = rect.Cells.ToList().FindAll(c => c.Roofed(map) && c.Standable(map) && c.GetEdifice(map) == null && c.GetFirstItem(map) == null).RandomElement();
			Thing thing = ThingMaker.MakeThing(thingDef);
			thing.stackCount = stackCount;
			thing.SetForbidden(true);

			GenSpawn.Spawn(thing, spawnPos, map, WipeMode.VanishOrMoveAside);
		}
	}
}
