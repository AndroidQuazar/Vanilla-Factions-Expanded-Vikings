using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEV
{
    public class CompPawnPsyWaver : ThingComp
    {

        private int readyToUseTicks = 0;
        public CompProperties_PawnPsyWaver Props
        {
            get
            {
                return (CompProperties_PawnPsyWaver)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent is Pawn pawn && pawn.Spawned && Find.TickManager.TicksGame >= readyToUseTicks)
            {
                readyToUseTicks = Find.TickManager.TicksGame + Props.interval.RandomInRange;
                var victims = pawn.Map.mapPawns.AllPawns.Where(x => x.Position.DistanceTo(pawn.Position) 
                < Props.maxDistance);
                MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, Props.maxDistance);
                foreach (var victim in victims)
                {
                    if (victim != pawn && !victim.RaceProps.IsMechanoid)
                    {
                        var stunPeriod = Props.stunPeriod;
                        if (victim.story?.traits?.GetTrait(TraitDefOf.PsychicSensitivity) != null)
                        {
                            stunPeriod /= victim.story.traits.GetTrait(TraitDefOf.PsychicSensitivity).Degree;
                        }
                        victim.stances.stunner.StunFor_NewTmp(stunPeriod, pawn, true);
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref readyToUseTicks, "readyToUseTicks", 0);
        }
    }
}

