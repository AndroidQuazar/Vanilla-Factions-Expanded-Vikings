﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace TNOR
{
    class JobDriver_TendToApiary : JobDriver
    {
        protected TNR_Apiary Apiary
        {
            get
            {
                return (TNR_Apiary)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Apiary;
            Job job = this.job;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(500, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.Apiary.needTend).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    int skill = pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").levelInt / 2;
                    Random rnd = new Random();
                    if (rnd.Next(0, 11 - skill) == 1)
                    {
                        this.Apiary.tickBeforeTend += 120000;
                    }
                    else
                    {
                        if (rnd.Next(1, 3) == 2)
                        {
                            pawn.TakeDamage(new DamageInfo(TNOR_DefOf.TNR_DamageSting, 2));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Stun, 20));
                        } 
                        pawn.needs.mood.thoughts.memories.TryGainMemoryFast(TNOR_DefOf.TNR_StingMoodDebuff);
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Tending failed", 5f);
                        pawn.jobs.StartJob(new Job(TNOR_DefOf.TNR_TendToApiary, TargetA));
                    }
                    pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").Learn(20, false);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}
