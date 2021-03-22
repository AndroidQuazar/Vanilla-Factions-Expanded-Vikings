using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEV
{
    public class VFEV_MapComponentHelper : MapComponent
    {

        public int lastFeastStartTick;
        public VFEV_MapComponentHelper(Map map) : base(map)
        {

        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (pawnsToTeleport != null && pawnsToTeleport.Count > 0)
            {
                List<Pawn> keysToRemove = new List<Pawn>();
                foreach (var pawnData in pawnsToTeleport)
                {
                    var teleportComp = pawnData.Key.TryGetComp<CompPawnTeleporter>();
                    if (teleportComp != null && teleportComp.disappear && Find.TickManager.TicksGame >= teleportComp.appearInTick)
                    {
                        GenPlace.TryPlaceThing(pawnData.Key, pawnData.Value, this.map,
                            ThingPlaceMode.Near, null, null, default(Rot4));
                        teleportComp.disappear = false;
                        if (teleportComp.Props.disableManhunterState 
                            && pawnData.Key.mindState.mentalStateHandler.CurStateDef 
                                == MentalStateDefOf.Manhunter)
                        {
                            pawnData.Key.mindState.mentalStateHandler.Reset();
                        }
                        keysToRemove.Add(pawnData.Key);
                        MoteMaker.MakeStaticMote(pawnData.Key.Position, this.map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
                        //Log.Message("APPEARED");
                    }
                }
                foreach (var pawn in keysToRemove)
                {
                    pawnsToTeleport.Remove(pawn);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref lastFeastStartTick, "lastFeastStartTick", 0);

            Scribe_Collections.Look<Pawn, IntVec3>(ref pawnsToTeleport, "pawnsToTeleport", 
                LookMode.Deep, LookMode.Value, ref this.pawnsToTeleportKeys,
                ref this.pawnsToTeleportValues);
        }

        public Dictionary<Pawn, IntVec3> pawnsToTeleport = new Dictionary<Pawn, IntVec3>();

        private List<Pawn> pawnsToTeleportKeys;

        private List<IntVec3> pawnsToTeleportValues;
    }
}

