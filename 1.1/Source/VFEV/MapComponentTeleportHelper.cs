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
                    if (teleportComp != null && teleportComp.disappear && Find.TickManager.TicksGame >= teleportComp.appearInTick)
                    {
                        GenPlace.TryPlaceThing(pawnData.Key, pawnData.Value.Cell, pawnData.Value.Map,
                            ThingPlaceMode.Near, null, null, default(Rot4));
                        keysToRemove.Add(pawnData.Key);
                        MoteMaker.MakeStaticMote(pawnData.Key.Position, pawnData.Key.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
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
            Scribe_Collections.Look(ref pawnsToTeleport, "pawnsToTeleport", LookMode.Reference, LookMode.TargetInfo);
        }

        public Dictionary<Pawn, TargetInfo> pawnsToTeleport = new Dictionary<Pawn, TargetInfo>();
    }
}

