using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEV
{
	public class LordJob_Joinable_Feast : LordJob_Joinable_Gathering
	{
		private int durationTicks;
		public override bool AllowStartNewGatherings => false;

		protected virtual ThoughtDef AttendeeThought => VFEV_DefOf.VFEV_AttendedFeast;

		protected virtual TaleDef AttendeeTale => TaleDefOf.AttendedParty;

		protected virtual ThoughtDef OrganizerThought => VFEV_DefOf.VFEV_AttendedFeast;

		protected virtual TaleDef OrganizerTale => TaleDefOf.AttendedParty;

		public int DurationTicks => durationTicks;

		public LordJob_Joinable_Feast()
		{
		}

		public LordJob_Joinable_Feast(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
			: base(spot, organizer, gatheringDef)
		{
			durationTicks = Rand.RangeInclusive(55000, 65000);
		}

		public override string GetReport(Pawn pawn)
		{
			return "LordReportAttendingParty".Translate();
		}

		protected override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
		{
			return new LordToil_Party(spot, gatheringDef);
		}

		public override StateGraph CreateGraph()
		{
		 //Log.Message("START: " + Find.TickManager.TicksGame, true);
			StateGraph stateGraph = new StateGraph();
			LordToil party = CreateGatheringToil(spot, organizer, gatheringDef);
			stateGraph.AddToil(party);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(party, lordToil_End);
			transition.AddTrigger(new Trigger_TickCondition(() => ShouldBeCalledOff()));
			transition.AddTrigger(new Trigger_PawnKilled());
			transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.IncappedOrKilled, organizer));
			transition.AddPreAction(new TransitionAction_Custom((Action)delegate
			{
				ApplyOutcome((LordToil_Party)party);
			}));
			transition.AddPreAction(new TransitionAction_Message(gatheringDef.calledOffMessage, MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, base.Map)));
			stateGraph.AddTransition(transition);
			timeoutTrigger = GetTimeoutTrigger();
			Transition transition2 = new Transition(party, lordToil_End);
			transition2.AddTrigger(timeoutTrigger);
			transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
			{
				ApplyOutcome((LordToil_Party)party);
			}));
			transition2.AddPreAction(new TransitionAction_Message(gatheringDef.finishedMessage, MessageTypeDefOf.SituationResolved, new TargetInfo(spot, base.Map)));
			stateGraph.AddTransition(transition2);
			return stateGraph;
		}

		protected override bool ShouldBeCalledOff()
		{
			if (!PawnCanStartOrContinueGathering(organizer))
			{
			 //Log.Message(" - ShouldBeCalledOff - return true; - 2", true);
				return true;
			}
			if (!AcceptableGameConditionsToContinueGathering(base.Map))
			{
			 //Log.Message(" - ShouldBeCalledOff - return true; - 4", true);
				return true;
			}
			return false;
		}

		public static bool AcceptableGameConditionsToContinueGathering(Map map)
		{
			if (map.dangerWatcher.DangerRating == StoryDanger.High)
			{
			 //Log.Message(" - AcceptableGameConditionsToContinueGathering - return false; - 7", true);
				return false;
			}
			return true;
		}


		public static bool PawnCanStartOrContinueGathering(Pawn pawn)
		{
			if (pawn.Drafted)
			{
			 //Log.Message(" - PawnCanStartOrContinueGathering - return false; - 2", true);
				return false;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0.3f)
			{
			 //Log.Message(" - PawnCanStartOrContinueGathering - return false; - 4", true);
				return false;
			}
			if (pawn.IsPrisoner)
			{
			 //Log.Message(" - PawnCanStartOrContinueGathering - return false; - 6", true);
				return false;
			}
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.2f)
			{
			 //Log.Message(" - PawnCanStartOrContinueGathering - return false; - 9", true);
				return false;
			}
			if (pawn.IsWildMan())
			{
			 //Log.Message(" - PawnCanStartOrContinueGathering - return false; - 11", true);
				return false;
			}

			//if (pawn.psychicEntropy != null && pawn.psychicEntropy.IsCurrentlyMeditating)
			//{
			//      return false;
			//}
			//if (pawn.Spawned && !pawn.Downed)
			//{
			//      return !pawn.InMentalState;
			//}
			return true;
		}



		protected virtual Trigger_TicksPassed GetTimeoutTrigger()
		{
			return new Trigger_TicksPassed(durationTicks);
		}

		private void ApplyOutcome(LordToil_Party toil)
		{
			List<Pawn> ownedPawns = lord.ownedPawns;
			LordToilData_Party lordToilData_Party = (LordToilData_Party)toil.data;
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				Pawn pawn = ownedPawns[i];
				bool flag = pawn == organizer;
				if (lordToilData_Party.presentForTicks.TryGetValue(pawn, out int value) && value > 0)
				{
					if (ownedPawns[i].needs.mood != null)
					{
						ThoughtDef thoughtDef = flag ? OrganizerThought : AttendeeThought;
						float num = 0.5f / thoughtDef.stages[0].baseMoodEffect;
						float moodPowerFactor = Mathf.Min((float)value / (float)durationTicks + num, 1f);
						Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
						thought_Memory.moodPowerFactor = moodPowerFactor;
						ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
					}
					TaleRecorder.RecordTale(flag ? OrganizerTale : AttendeeTale, ownedPawns[i], organizer);
				}
			}
		}
        public override void Cleanup()
        {
            base.Cleanup();
		 //Log.Message("END: " + Find.TickManager.TicksGame, true);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref durationTicks, "durationTicks", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && gatheringDef == null)
			{
				gatheringDef = DefDatabase<GatheringDef>.GetNamed("VFEV_Feast");
			}
		}
	}
}
