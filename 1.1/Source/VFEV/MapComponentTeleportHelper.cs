using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEV
{
    public class MapComponentTeleportHelper : MapComponent
    {
        public MapComponentTeleportHelper(Map map) : base(map)
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
                    //Log.Message("teleportComp.appearInTick: " + teleportComp.appearInTick);
                    if (teleportComp != null && teleportComp.disappear && Find.TickManager.TicksGame >= teleportComp.appearInTick)
                    {
                        GenPlace.TryPlaceThing(pawnData.Key, pawnData.Value, this.map,
                            ThingPlaceMode.Near, null, null, default(Rot4));
                        teleportComp.disappear = false;
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
            Scribe_Collections.Look<Pawn, IntVec3>(ref pawnsToTeleport, "pawnsToTeleport", 
                LookMode.Deep, LookMode.Value, ref this.pawnsToTeleportKeys,
                ref this.pawnsToTeleportValues);
        }

        public Dictionary<Pawn, IntVec3> pawnsToTeleport = new Dictionary<Pawn, IntVec3>();

        private List<Pawn> pawnsToTeleportKeys;

        private List<IntVec3> pawnsToTeleportValues;
    }
}

