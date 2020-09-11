using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEV
{
    public class IncidentWorker_SlaverRaid : IncidentWorker_RaidEnemy
    {
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            parms.faction = Find.FactionManager.FirstFactionOfDef(VFEV_DefOf.VFEV_VikingsSlaver);
            return true;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        }

        private void GenerateAnimals(Faction faction, List<Pawn> pawns)
        {
            int num = (int)((float)pawns.Count * 0.7f);
            for (int i = 0; i < num; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDef.Named("VFEV_Wolfhound"), faction));
                pawns.Add(pawn);
            }
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ResolveRaidPoints(parms);
            if (!this.TryResolveRaidFaction(parms))
            {
                return false;
            }
            if (parms.faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally
                || parms.faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Neutral)
            {
                return false;
            }
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            this.ResolveRaidStrategy(parms, combat);
            this.ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            parms.points = IncidentWorker_Raid.AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list != null) GenerateAnimals(parms.faction, list);

            if (list == null)
            {
                var groupMaker = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms, false);
                //Log.Message("groupMaker: " + groupMaker.groupKind, true);
                //Log.Message("groupMaker.raidStrategy: " + groupMaker.raidStrategy, true);
                //Log.Message("groupMaker.points: " + groupMaker.points, true);
                list = PawnGroupMakerUtility.GeneratePawns(groupMaker, true).ToList<Pawn>();
                foreach (var p in list)
                {
                    //Log.Message("parms.pawnKind: " + p.kindDef, true);
                }
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms, false);
                    return false;
                }
                GenerateAnimals(parms.faction, list);
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
            foreach (Pawn pawn in list)
            {
                string str = (pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.LabelCap : "unarmed";
                stringBuilder.AppendLine(pawn.KindLabel + " - " + str);
            }
            TaggedString baseLetterLabel = this.GetLetterLabel(parms);
            TaggedString baseLetterText = this.GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref baseLetterLabel, ref baseLetterText, this.GetRelatedPawnsInfoLetterText(parms), true, true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any<Pawn>())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any<Pawn>())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any<Pawn>())
            {
                foreach (Pawn t in list)
                {
                    list2.Add(t);
                }
            }
            base.SendStandardLetter(baseLetterLabel, baseLetterText, this.GetLetterDef(), parms, list2, Array.Empty<NamedArgument>());
            
            var lord = new LordJob_AssaultColony(parms.faction, false, true, true, true, false);
            LordMaker.MakeNewLord(parms.faction, lord, (Map)parms.target, list);

            foreach (var pawn in list)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("VFEV_CaptureDownedVictimAndLeaveMap"));
                }
                else
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                }
            }
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].apparel.WornApparel.Any((Apparel ap) => ap is ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            return true;
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return base.def.letterLabel;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            return "VFEV.SlaverRaidDesc".Translate(parms.faction);
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsRaidEnemy", Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}

