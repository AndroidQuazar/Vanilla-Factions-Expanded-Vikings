using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEV
{
    public class CompPawnTeleporter : ThingComp
    {
        public bool disappear = false;

        public int appearInTick = 0;

        private int readyToUseTicks = 0;
        public CompProperties_PawnTeleporter Props
        {
            get
            {
                return (CompProperties_PawnTeleporter)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent is Pawn pawn && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f
                    && Find.TickManager.TicksGame >= readyToUseTicks)
            {
                var hostiles = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget).Where(x
                    => x.HostileTo(pawn));
                readyToUseTicks = Find.TickManager.TicksGame + Props.cooldown;
                IntVec3 loc = IntVec3.Invalid;
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) =>
                    hostiles.Where(y => y.Position.DistanceTo(x) > Props.minDistance).Count() == 0 && x.Walkable(this.parent.Map) && !x.Fogged(this.parent.Map), 
                    pawn.Map, out loc))
                {
                    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
                    disappear = true;
                    appearInTick = Find.TickManager.TicksGame + 120;
                    var mapComp = pawn.Map.GetComponent<VFEV_MapComponentHelper>();
                    if (mapComp.pawnsToTeleport == null) mapComp.pawnsToTeleport = new Dictionary<Pawn, IntVec3>();
                    mapComp.pawnsToTeleport[pawn] = loc;
                    pawn.DeSpawn(DestroyMode.Vanish);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref readyToUseTicks, "readyToUseTicks", 0);
            Scribe_Values.Look<int>(ref appearInTick, "appearInTick", 0);
            Scribe_Values.Look<bool>(ref disappear, "disappear", false);
        }
    }
}

