using System;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using Verse.Sound;

namespace VFEV
{
    class JobDriver_TrainAtDummy : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public int initialXP = 0;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                initialXP = TargetThingA.HitPoints;
            };
            toil.tickAction = delegate ()
            {
                this.pawn.rotationTracker.FaceTarget(base.TargetA);
                this.pawn.GainComfortFromCellIfPossible(false);
                if (this.pawn.meleeVerbs.TryMeleeAttack(TargetA.Thing))
                {
                    //SoundInfo var = SoundInfo.InMap(new TargetInfo(this.TargetThingA));
                    //var.volumeFactor = 0.25f;
                    //SoundDef.Named("Pawn_Melee_Punch_HitBuilding_Quiet").PlayOneShot(var);
                    this.pawn.skills.Learn(SkillDefOf.Melee, 30f, false);
                    TargetThingA.HitPoints = initialXP;
                }
                Building joySource = (Building)base.TargetThingA;
                JoyUtility.JoyTickCheckEnd(this.pawn, this.job.doUntilGatheringEnded ? JoyTickFullJoyAction.None : JoyTickFullJoyAction.EndJob, 1f, joySource);
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

        public override object[] TaleParameters()
        {
            return new object[2]
            {
                pawn,
                base.TargetA.Thing.def
            };
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initialXP, "initialXP", 0);
        }
    }
}