using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEV.Facepaint
{
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class JobDriver_ChangeFacepaint : JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TableIndex), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Go to hairdressing table
            Toil gotoToil = Toils_Goto.GotoThing(TableIndex, PathEndMode.InteractionCell);
            yield return gotoToil;

            // Bring up interface
            yield return new Toil()
                         {
                             initAction          = () => { Find.WindowStack.Add(new Dialog_ChangeFacepaint(this)); },
                             defaultCompleteMode = ToilCompleteMode.Never
                         };

            // Change hairstyle
            Toil hairdressToil = new Toil
                                 {
                                     tickAction = () =>
                                                  {
                                                      // Work on changing hairstyle
                                                      restyleTicksDone += pawn.GetStatValue(RimWorld.StatDefOf.GeneralLaborSpeed);
                                                      if (restyleTicksDone >= ticksToRestyle)
                                                      {
                                                          //if (AnyChanges)
                                                          ;//FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.VHE_Filth_Hair, 3);


                                                          if (pawn.GetComp<CompFacepaint>() is CompFacepaint facepaintComp)
                                                          {
                                                              facepaintComp.facepaintDefOne = this.newFacepaintDefOne;
                                                              if (this.newFacepaintColorOne.HasValue)
                                                                  facepaintComp.colorOne = this.newFacepaintColorOne.Value;
                                                              facepaintComp.facepaintDefTwo = this.newFacepaintDefTwo;
                                                              if (this.newFacepaintColorTwo.HasValue)
                                                                  facepaintComp.colorTwo = this.newFacepaintColorTwo.Value;
                                                          }

                                                          pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                                                          PortraitsCache.SetDirty(pawn);
                                                          pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                                                      }
                                                  },
                                     defaultCompleteMode = ToilCompleteMode.Never
                                 };

            hairdressToil.WithProgressBar(TableIndex, () => restyleTicksDone / ticksToRestyle, true);
            hairdressToil.FailOnCannotTouch(TableIndex, PathEndMode.Touch);
            hairdressToil.PlaySustainerOrSound(SoundDefOf.TinyBell);
            yield return hairdressToil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref this.newFacepaintDefOne, "newFacepaintDefOne");
            Scribe_Values.Look(ref this.newFacepaintColorOne, "newFacepaintColorOne");
            Scribe_Defs.Look(ref this.newFacepaintDefTwo, "newFacepaintDefTwo");
            Scribe_Values.Look(ref this.newFacepaintColorTwo,   "newFacepaintColorTwo");
            Scribe_Values.Look(ref ticksToRestyle,   "ticksToRestyle");
            Scribe_Values.Look(ref restyleTicksDone, "restyleTicksDone");
        }

        private const TargetIndex TableIndex = TargetIndex.A;

        //private bool AnyChanges => this.newFacepaintDefOne != null || this.newFacepaintColorOne.HasValue || this.newFacepaintDefTwo != null || this.newFacepaintColorTwo.HasValue;

        public  FacepaintDef newFacepaintDefOne;
        public  Color?  newFacepaintColorOne;
        public  FacepaintDef newFacepaintDefTwo;
        public  Color?  newFacepaintColorTwo;
        public  int     ticksToRestyle;
        private float   restyleTicksDone;

    }
}