using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using Verse.AI;
using RimWorld;

namespace VFEV
	{
		class JobDriver_PlayHnefatafl : JobDriver_SitFacingBuilding
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
					this.MayStartSocialFight(this.pawn);
				});
				this.ModifyPlayToil(toil);
				yield return toil;
				yield break;
			}

			private void MayStartSocialFight(Pawn pawn)
			{
			if (Rand.RangeInclusive(1, 100) <= 5)
			{
				IntVec3 cell = pawn.Position;
				if (pawn.Rotation == Rot4.South) cell.z -= 2;
				else if (pawn.Rotation == Rot4.North) cell.z += 2;
				else if (pawn.Rotation == Rot4.West) cell.x -= 2;
				else if (pawn.Rotation == Rot4.East) cell.x += 2;

				Pawn otherPlayer = cell.GetFirstPawn(pawn.Map);
				if (otherPlayer != null && otherPlayer.IsColonist)
                {
					pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, "Due to the fierce game of hnefatafl players engaged in a social fight", false, false, otherPlayer);
					otherPlayer.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, "Due to the fierce game of hnefatafl players engaged in a social fight", false, false, pawn);
				}
			}
		}
	}
}

