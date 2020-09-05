using RimWorld;
using Verse;

namespace VFEV
{
	public class InteractionWorker_DrunkChat : InteractionWorker
	{
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
            var alcoholHediff = initiator.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
            if (alcoholHediff != null && initiator.mindState?.duty?.def == VFEV_DefOf.VFEV_Feast)
            {
                if (alcoholHediff.CurStageIndex == 1) //tipsy
                {
                    return 1f;
                }
                if (alcoholHediff.CurStageIndex == 2) //drunk
                {
                    return 2f;
                }
            }
            return 0f;
        }
	}
}
