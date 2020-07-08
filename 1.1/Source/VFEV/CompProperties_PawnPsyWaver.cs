using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEV
{
	public class CompProperties_PawnPsyWaver : CompProperties
	{
		public CompProperties_PawnPsyWaver()
		{
			this.compClass = typeof(CompPawnPsyWaver);
		}

		public float maxDistance;

		public IntRange interval;

		public int stunPeriod;
	}
}

