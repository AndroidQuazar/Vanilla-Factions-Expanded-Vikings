using System;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using Verse.Sound;

namespace VFEV
{
    class JobDriver_TrainAtDummy : JobDriver_WatchBuilding
    {
		protected override void WatchTickAction()
		{
			if (this.pawn.IsHashIntervalTick(400))
			{
				Verb meleeVerb = pawn.TryGetAttackVerb(this.TargetThingA);
				if (meleeVerb != null)
                {
					SoundDef.Named("Pawn_Melee_Punch_HitBuilding").PlayOneShot(new TargetInfo(this.TargetThingA));
					pawn.skills.Learn(SkillDefOf.Melee, 30f, false);
				}
			}
			base.WatchTickAction();
		}
	}
}
