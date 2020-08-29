using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEV
{
    class JoyGiver_PlayTrainingDummy : JoyGiver
    {
		private static List<Thing> candidates = new List<Thing>();

		public override Job TryGiveJob(Pawn pawn)
		{
			bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn, null);
			Job result;
			try
			{
				JoyGiver_PlayTrainingDummy.candidates.AddRange(pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(delegate (Thing thing)
				{
					if (thing.Faction != Faction.OfPlayer || (!allowedOutside && !thing.Position.Roofed(thing.Map)) || !pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None, 1, -1, null, false) && thing.def.defName != "VFEV_TrainingDummy")
					{
						return false;
					}
					return true;
				}));
				Thing t = candidates.RandomElement();
				if (t == null)
				{
					result = null;
				}
				else
				{
					result = JobMaker.MakeJob(this.def.jobDef, t);
				}
			}
			finally
			{
				JoyGiver_PlayTrainingDummy.candidates.Clear();
			}
			return result;
        }
	}
}
