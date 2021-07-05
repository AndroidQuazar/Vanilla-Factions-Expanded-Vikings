using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEV
{
	[StaticConstructorOnStartup]
	class LightningStrike : OrbitalStrike
	{
		public override void StartStrike()
		{
			this.duration = this.lightningIntervalTick * 50;
			base.StartStrike();
		}

		public override void Tick()
		{
			if (base.Destroyed)
			{
				return;
			}
			if (this.warmupTicks > 0)
			{
				this.warmupTicks--;
				if (this.warmupTicks == 0)
				{
					this.StartStrike();
				}
			}
			else
			{
				base.Tick();
			}
			this.EffectTick();
		}

		private void EffectTick()
		{
			if (!this.nextLightningCell.IsValid)
			{
				this.ticksToNextEffect = this.warmupTicks - this.lightningIntervalTick;
				this.GetnextLightningCell();
			}
			this.ticksToNextEffect--;
			if (this.ticksToNextEffect <= 0 && base.TicksLeft >= this.lightningIntervalTick && this.lightningLeftCount < 51)
			{
				this.ticksToNextEffect = this.lightningIntervalTick;
				base.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(base.Map, nextLightningCell));
				this.GetnextLightningCell();
				this.lightningLeftCount--;
			}
		}

		private void GetnextLightningCell()
		{
			this.nextLightningCell = (from x in GenRadial.RadialCellsAround(base.Position, this.impactAreaRadius, true)
									  where x.InBounds(base.Map)
									  select x).RandomElementByWeight((IntVec3 x) => Bombardment.DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position) / this.impactAreaRadius));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.impactAreaRadius, "impactAreaRadius", 15f, false);
			Scribe_Values.Look<FloatRange>(ref this.lightningRadiusRange, "lightningRadiusRange", new FloatRange(6f, 8f), false);
			Scribe_Values.Look<int>(ref this.lightningIntervalTick, "lightningIntervalTick", 18, false);
			Scribe_Values.Look<int>(ref this.warmupTicks, "warmupTicks", 0, false);
			Scribe_Values.Look<int>(ref this.ticksToNextEffect, "ticksToNextEffect", 0, false);
			Scribe_Values.Look<int>(ref this.lightningLeftCount, "lightningLeftCount", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.nextLightningCell, "nextLightningCell", default(IntVec3), false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (!this.nextLightningCell.IsValid)
				{
					this.GetnextLightningCell();
				}
			}
		}

		public float impactAreaRadius = 14f;

		public FloatRange lightningRadiusRange = new FloatRange(6f, 8f);

		public int lightningIntervalTick = 20;

		public int warmupTicks = 60;

		public int lightningLeftCount = 50;

		private int ticksToNextEffect;

		private IntVec3 nextLightningCell = IntVec3.Invalid;
	}
}
