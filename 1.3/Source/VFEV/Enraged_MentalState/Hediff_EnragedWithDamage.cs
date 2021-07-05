using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEV
{
    public class Hediff_EnragedWithDamage : HediffWithComps
    {
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (Rand.Chance(0.75f))
            {
                VFEV_DefOf.VFEV_Enraged.Worker.TryStart(this.pawn, "VFEV.CausedByDamage".Translate(), false);
            }
        }
    }
}

