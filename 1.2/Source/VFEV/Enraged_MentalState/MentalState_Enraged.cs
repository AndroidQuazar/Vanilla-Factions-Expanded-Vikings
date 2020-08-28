using RimWorld;
using Verse;
using Verse.AI;

namespace VFEV
{
	public class MentalState_Enraged : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
