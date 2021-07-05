using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEV
{

    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("VFEV.OskarPotocki").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Toils_LayDown))]
    class PatchToils_LayDown
    {
        [HarmonyPostfix]
        [HarmonyPatch("ApplyBedThoughts")]
        [HarmonyPatch(new Type[] { typeof(Pawn) })]
        static void PostFix(Pawn actor)
        {
            Building_Bed building_Bed = actor.CurrentBed();
            if (building_Bed != null && building_Bed.def.defName.Contains("FurBed")) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
        }
    }

    [HarmonyPatch(typeof(RaceProperties))]
    class PatchIsMechanoid
    {
        [HarmonyPostfix]
        [HarmonyPatch("IsMechanoid", MethodType.Getter)]
        static void PostFix(ref RaceProperties __instance, ref bool __result)
        {
            if (__instance.FleshType == FleshTypeDefOf.Mechanoid && __instance?.body.defName == "VFEV_Odin") __result = false;
        }
    }
}