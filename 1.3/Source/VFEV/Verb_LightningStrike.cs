using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace VFEV
{
    class Verb_LightningStrike : Verb_CastBase
    {
		protected override bool TryCastShot()
		{
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			LightningStrike lightningStrike = (LightningStrike)GenSpawn.Spawn(VFEV_DefOf.VFEV_LightningStrike, this.currentTarget.Cell, this.caster.Map, WipeMode.Vanish);
			lightningStrike.duration = 540;
			lightningStrike.instigator = this.caster;
			lightningStrike.weaponDef = ((base.EquipmentSource != null) ? base.EquipmentSource.def : null);
			if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
			{
				base.EquipmentSource.Destroy(DestroyMode.Vanish);
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return 23f;
		}

		public const int DurationTicks = 540;
	}
}
