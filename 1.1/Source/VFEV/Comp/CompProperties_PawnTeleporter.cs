using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEV
{
	public class CompProperties_PawnTeleporter : CompProperties
	{
		public CompProperties_PawnTeleporter()
		{
			this.compClass = typeof(CompPawnTeleporter);
		}

		public float minDistance;

		public int cooldown;

		public bool disableManhunterState = false;
	}
}

