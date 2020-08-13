using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEV.Facepaint
{
    using RimWorld;
    using Verse;
    using Verse.AI;

    public class Building_FacepaintingTable : Building
    {

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.IsColonistPlayerControlled)
            {
                yield return new FloatMenuOption("VanillaFactionsExpandedVikings.ChangeFacepaint".Translate(),
                                                 () => selPawn.jobs.TryTakeOrderedJob(new Job(VFEV_DefOf.VFEV_ChangeFacepaint, this)));
            }
        }

    }
}
