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
            if (Rand.RangeInclusive(0, 100) >= 50 && hitThing is Pawn pawn)
            {
                pawn.stances.stunner.StunFor_NewTmp(250, null, false, true);
            }
            base.Impact(hitThing);
        }
    }
}
