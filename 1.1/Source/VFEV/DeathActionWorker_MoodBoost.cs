using RimWorld;
using System;
using Verse;

namespace VFEV
{
    class DeathActionWorker_MoodBoost : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            foreach (Map map in Find.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.FreeColonists)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(VFEV_DefOf.VFEV_BeastHunted);
                }
            }
        }
    }
}
