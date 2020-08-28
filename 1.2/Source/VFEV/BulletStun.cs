using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEV
{
    class BulletStun : Bullet
    {
        protected override void Impact(Thing hitThing)
        {
            if (Rand.RangeInclusive(1, 100) >= 75 && hitThing is Pawn pawn && this.ParentHolder is Pawn instigator)
            {
                pawn.stances.stunner.StunFor_NewTmp(100, instigator);
            }
            base.Impact(hitThing);
        }
    }
}
