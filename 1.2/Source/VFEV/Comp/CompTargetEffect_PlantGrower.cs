using System.Linq;
using RimWorld;
using Verse;

namespace VFEV
{
	public class CompTargetEffect_PlantGrower : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			foreach (Thing plant in user.MapHeld.listerThings.AllThings.Where(x => x is Plant))
			{
				if (((Plant)plant).Growth < 0.9f && ((Plant)plant).LifeStage != PlantLifeStage.Sowing)
				{
					((Plant)plant).Growth = 0.9f;
					plant.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
				}
			}
		}
	}
}
