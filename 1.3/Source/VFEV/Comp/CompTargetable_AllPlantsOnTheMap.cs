using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEV
{
	public class CompTargetable_AllPlantsOnTheMap : CompTargetable
	{
		protected override bool PlayerChoosesTarget
		{
			get
			{
				return false;
			}
		}
		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
			yield break;
		}

		protected override TargetingParameters GetTargetingParameters()
		{
		 //Log.Message("TEST");
			return new TargetingParameters
			{
				validator = (TargetInfo x) => x.Thing is Plant
			};
		}
	}
}
