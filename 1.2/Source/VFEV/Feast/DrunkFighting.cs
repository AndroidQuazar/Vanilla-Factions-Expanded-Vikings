using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEV
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "SocialFightChance")]
    internal static class SocialFightChance
    {
        public static void Prefix(Pawn_InteractionsTracker __instance, Pawn ___pawn, ref InteractionDef interaction, Pawn initiator)
        {
            var alcoholHediff = ___pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
            if (alcoholHediff != null && ___pawn.Map.GetComponent<VFEV_MapComponentHelper>().feastIsActive)
            {
                if (alcoholHediff.CurStageIndex == 2) //drunk
                {
                    interaction = VFEV_DefOf.VFEV_DrunkChitchat;
                }
                else if (alcoholHediff.CurStageIndex == 3) // hammered
                {
                    interaction = VFEV_DefOf.VFEV_VeryDrunkChitchat;
                }
            }
        }
    }
}
