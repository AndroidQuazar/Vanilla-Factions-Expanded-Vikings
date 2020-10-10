using RimWorld;
using Verse;

namespace VFEV
{
	public class ThoughtWorker_TakingPartInFeast : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.mindState?.duty?.def == VFEV_DefOf.VFEV_Feast;
		}
	}
}
