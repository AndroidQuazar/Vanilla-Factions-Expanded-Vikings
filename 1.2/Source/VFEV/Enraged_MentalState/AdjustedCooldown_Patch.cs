using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEV
{
    [HarmonyPatch(typeof(Tool), "AdjustedCooldown")]
    internal static class AdjustedCooldown_Patch
    {
        public static void Postfix(Thing ownerEquipment, ref float __result)
        {
            if (ownerEquipment?.ParentHolder is Pawn_EquipmentTracker eq && eq.pawn.MentalStateDef == VFEV_DefOf.VFEV_Enraged.mentalState) 
            {
                __result = __result / 2f; // reduces time between melee attacks by 2
            }
        }
    }
}
