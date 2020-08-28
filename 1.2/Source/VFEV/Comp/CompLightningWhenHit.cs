using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEV.Comp
{
    class CompLightningWhenHit : ThingComp
    {
        public Comp.CompProperties_LightningWhenHit Props
        {
            get
            {
                return (CompProperties_LightningWhenHit)this.props;
            }
        }

        private float pastHealtSummaryPercent = 1f;

        public override void CompTick()
        {
            base.CompTick();
            Pawn odin = this.parent as Pawn;
            if (odin != null && odin.Spawned)
            {
                if (odin.health.summaryHealth.SummaryHealthPercent < pastHealtSummaryPercent)
                {
                    for (int i = 0; i < Rand.RangeInclusive(1, 5); i++)
                    {
                        IntVec3 nextLightningCell = GenRadial.RadialCellsAround(this.parent.Position, 5f, true).ToList().Find(x => x.InBounds(this.parent.Map));
                        this.parent.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(this.parent.Map, nextLightningCell));
                    }
                }
                pastHealtSummaryPercent = odin.health.summaryHealth.SummaryHealthPercent;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.pastHealtSummaryPercent, "pastHealtSummaryPercent", 1f, false);
        }
    }
}
