using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace VFEV
{
    class SitePartWorker_LegendaryGrave : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            Faction faction = Find.FactionManager.AllFactionsListForReading.Find(f => !f.defeated && !f.IsPlayer && f.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile && f.def.defName.Contains("VFEV_Vikings"));
            if (faction == null) faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Neolithic);
            part.site.SetFaction(faction);
        }
    }
}
