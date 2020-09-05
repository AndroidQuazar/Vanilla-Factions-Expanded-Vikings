using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEV
{
	public class InteractionWorker_VeryDrunkChat : InteractionWorker
	{
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
            var alcoholHediff = initiator.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
            if (alcoholHediff != null && initiator.mindState?.duty?.def == VFEV_DefOf.VFEV_Feast)
            {
                if (alcoholHediff.CurStageIndex == 3) // hammered
                {
                    return 2f;
                }
            }
            return 0f;
        }
    }
}
