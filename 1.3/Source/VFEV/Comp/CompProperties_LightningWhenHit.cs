using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEV.Comp
{
    class CompProperties_LightningWhenHit : CompProperties
    {
        public CompProperties_LightningWhenHit()
        {
            this.compClass = typeof(Comp.CompLightningWhenHit);
        }
    }
}
