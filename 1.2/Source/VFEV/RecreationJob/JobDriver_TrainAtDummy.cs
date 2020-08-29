using System;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using Verse.Sound;

namespace VFEV
{
    class JobDriver_TrainAtDummy : JobDriver_WatchBuilding
    {
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
			Toil toil = new Toil();
			toil.tickAction = delegate ()
			{
				this.pawn.rotationTracker.FaceTarget(base.TargetA);
				this.pawn.GainComfortFromCellIfPossible(false);
				Pawn pawn = this.pawn;
				Building joySource = (Building)base.TargetThingA;
				JoyUtility.JoyTickCheckEnd(pawn, this.job.doUntilGatheringEnded ? JoyTickFullJoyAction.None : JoyTickFullJoyAction.EndJob, 1f, joySource);
			};
			toil.handlingFacing = true;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = (this.job.doUntilGatheringEnded ? this.job.expiryInterval : this.job.def.joyDuration);
			toil.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.pawn);
			});
			yield return toil;
			yield break;
		}

		protected override void WatchTickAction()
		{
			if (this.pawn.IsHashIntervalTick(400))
			{
				Verb meleeVerb = pawn.TryGetAttackVerb(this.TargetThingA);
				if (meleeVerb != null)
                {
					SoundDef.Named("Pawn_Melee_Punch_HitBuilding").PlayOneShot(new TargetInfo(this.TargetThingA));
					pawn.skills.Learn(SkillDefOf.Melee, 30f, false);
				}
			}
			base.WatchTickAction();
		}
	}
}
