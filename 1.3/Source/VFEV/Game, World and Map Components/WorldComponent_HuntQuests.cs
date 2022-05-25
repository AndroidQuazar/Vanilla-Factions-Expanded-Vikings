using RimWorld;
using RimWorld.Planet;
using Verse;
using RimWorld.QuestGen;
using System.Collections.Generic;

namespace VFEV
{



    public class WorldComponent_HuntQuests : WorldComponent
    {
        public int tickCounterStructure;
        public int tickCounterHunt;
        public int ticksToNextStructureQuest = 60000 * 60;
        public int ticksToNextHuntQuest = 60000 * 55;


        public WorldComponent_HuntQuests(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();



        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();



            if (tickCounterStructure > ticksToNextStructureQuest)
            {
                List<QuestScriptDef> questList = new List<QuestScriptDef>(){ VFEV_DefOf.VFEV_OpportunitySite_LegendaryGrave, VFEV_DefOf.VFEV_OpportunitySite_AncientRuin};

                Slate slate = new Slate();
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questList.RandomElement(), slate);
                QuestUtility.SendLetterQuestAvailable(quest);
                ticksToNextStructureQuest = (int)(60000 * Rand.RangeInclusive(50, 70) *VFEV_Mod.settings.VFEV_StructureQuestMultiplier);
                tickCounterStructure = 0;




            }
            tickCounterStructure++;

            if (tickCounterHunt > ticksToNextHuntQuest)
            {
                List<QuestScriptDef> questList = new List<QuestScriptDef>() { VFEV_DefOf.VFEV_LothurrHunt, VFEV_DefOf.VFEV_NjorunHunt, VFEV_DefOf.VFEV_FenrirHunt, VFEV_DefOf.VFEV_OdinHunt, VFEV_DefOf.VFEV_ThrumboHunt };
                Slate slate = new Slate();
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questList.RandomElement(), slate);

                QuestUtility.SendLetterQuestAvailable(quest);
                ticksToNextHuntQuest = (int)(60000 * Rand.RangeInclusive(50, 70) * VFEV_Mod.settings.VFEV_HuntQuestMultiplier);
                tickCounterHunt = 0;




            }
            tickCounterHunt++;





        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.tickCounterStructure, nameof(this.tickCounterStructure));
            Scribe_Values.Look(ref this.tickCounterHunt, nameof(this.tickCounterHunt));
            Scribe_Values.Look(ref this.ticksToNextStructureQuest, nameof(this.ticksToNextStructureQuest));
            Scribe_Values.Look(ref this.ticksToNextHuntQuest, nameof(this.ticksToNextHuntQuest));
        }
    }
}
