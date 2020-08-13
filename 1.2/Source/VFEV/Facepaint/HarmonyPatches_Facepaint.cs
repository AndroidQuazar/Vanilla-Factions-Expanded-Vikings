using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEV.Facepaint
{
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public static class HarmonyPatches_Facepaint
    {
        static HarmonyPatches_Facepaint()
        {
            Harmony harmony = new Harmony("OskarPotocki.VanillaFactionsExpandedVikings");
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal",new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) }),
                          transpiler: new HarmonyMethod(typeof(HarmonyPatches_Facepaint), methodName: nameof(RenderPawnInternalTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGraphicSet), name: nameof(PawnGraphicSet.ResolveAllGraphics)),
                          postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches_Facepaint), methodName: nameof(ResolveAllGraphicsPostfix)));
        }

        public static IEnumerable<CodeInstruction> RenderPawnInternalTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionList = instructions.ToList();
            bool hideHairDeclared = false;
            bool hideHairAssigned = false;
            bool drawFacepaintCall = false;

            var hideFacepaint = generator.DeclareLocal(typeof(bool));

            var getItemInfo = AccessTools.Method(typeof(List<ApparelGraphicRecord>),"get_Item");

            var getHatsOnlyOnMapInfo = AccessTools.Property(typeof(Prefs), nameof(Prefs.HatsOnlyOnMap)).GetGetMethod();

            var shouldHideHeadgearInfo = AccessTools.Method(typeof(HarmonyPatches_Facepaint), nameof(ShouldHideHeadgear));
            var shouldHideFacepaintInfo = AccessTools.Method(typeof(HarmonyPatches_Facepaint), nameof(ShouldHideFacepaint));
            var renderFacepaint = AccessTools.Method(typeof(HarmonyPatches_Facepaint), nameof(RenderFacepaint));

            for (int i = 0; i <= instructionList.Count - 1; i++)
            {
                var instruction = instructionList[i];

                // Looking for calls to Prefs.HatsOnlyOnMap
                if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == getHatsOnlyOnMapInfo)
                {
                    yield return instruction;
                    instruction = new CodeInstruction(OpCodes.Call, shouldHideHeadgearInfo);
                }

                // Looking for any assignments to 'bool flag'
                if (instruction.operand is LocalBuilder lb && lb.LocalIndex == 14)
                {
                    if (instruction.opcode == OpCodes.Stloc_S)
                    {
                        var prevInstruction = instructionList[i - 1];
                        if (!hideHairDeclared && prevInstruction.opcode == OpCodes.Ldc_I4_0)
                        {
                            yield return instruction; // bool flag = false
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0); // false
                            instruction = new CodeInstruction(OpCodes.Stloc_S, hideFacepaint.LocalIndex); // hideFacepaint = false
                            hideHairDeclared = true;
                        }
                        if (!hideHairAssigned && prevInstruction.opcode == OpCodes.Ldc_I4_1)
                        {
                            yield return instruction; // flag = true
                            yield return new CodeInstruction(OpCodes.Ldloc_S, hideFacepaint.LocalIndex); // hideFacepaint
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // apparelGraphics
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 16); // j
                            yield return new CodeInstruction(OpCodes.Callvirt, getItemInfo); // apparelGraphics[j]
                            yield return new CodeInstruction(OpCodes.Call, shouldHideFacepaintInfo); // ShouldHideFacepaint(hideFacepaint, apparelGraphics[j])
                            instruction = new CodeInstruction(OpCodes.Stloc_S, hideFacepaint.LocalIndex); // hideFacepaint = ShouldHideFacepaint(hideFacepaint, apparelGraphics[j])
                            hideHairAssigned = true;
                        }
                    }else if (!drawFacepaintCall && hideHairAssigned && instruction.opcode == OpCodes.Ldloc_S)
                    {

                        yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = instruction.ExtractLabels()};                           // this
                        yield return new CodeInstruction(OpCodes.Ldloc_S, hideFacepaint.LocalIndex); // hideFacepaint
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 6);                        // bodyDrawType
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 8);                        // headStump
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);                        // headFacing
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 13);                       // loc2
                        yield return new CodeInstruction(OpCodes.Ldloc_0);                           // quaternion
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 7);                        // portrait
                        yield return new CodeInstruction(OpCodes.Call,    renderFacepaint);          // RenderFacepaint(this, hideFacepaint, bodyDrawType, headStump, headFacing, loc2, quaternion, portrait)
                        drawFacepaintCall = true;
                    }
                }


                yield return instruction;
            }
        }

        private static bool ShouldHideHeadgear(bool original)
        {
            // Also check if there are any change hairstyle windows currently open
            return original || Find.WindowStack.Windows.Any(w => w is Dialog_ChangeFacepaint);
        }

        private static bool ShouldHideFacepaint(bool hidden, ApparelGraphicRecord graphicRecord)
        {
            // Check if apparel covers any parts that should hide beards
            if (!hidden)
            {
                return false; // graphicRecord.sourceApparel.def.apparel.bodyPartGroups.Any(g => BodyPartGroupDefExtension.Get(g).hideBeardIfCovered);
            }
            return hidden;
        }

        private static void RenderFacepaint(PawnRenderer instance, bool hideFacepaint, RotDrawMode bodyDrawType, bool headStump, Rot4 headFacing, Vector3 baseDrawPos, Quaternion quaternion, bool portrait)
        {
            var pawn = instance.graphics.pawn;
            if (bodyDrawType != RotDrawMode.Dessicated && !headStump &&
                pawn.GetComp<CompFacepaint>() is CompFacepaint facepaintComp)
            {
                Mesh mesh = instance.graphics.HairMeshSet.MeshAt(headFacing);
                if (facepaintComp.facepaintGraphicOne != null) 
                    GenDraw.DrawMeshNowOrLater(mesh, baseDrawPos - new Vector3(0, 0.0007f, 0), quaternion, facepaintComp.facepaintGraphicOne.MatAt(headFacing), portrait);
                if (facepaintComp.facepaintGraphicTwo != null)
                    GenDraw.DrawMeshNowOrLater(mesh, baseDrawPos - new Vector3(0, 0.0005f, 0), quaternion, facepaintComp.facepaintGraphicTwo.MatAt(headFacing), portrait);
            }
        }

        public static void ResolveAllGraphicsPostfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (!pawn.RaceProps.Humanlike) return;
            CompFacepaint compFacepaint = pawn.GetComp<CompFacepaint>();
            if (compFacepaint == null) return;
            compFacepaint.facepaintGraphicOne =  compFacepaint.facepaintDefOne?.Graphic.GetColoredVersion(compFacepaint.facepaintDefOne.shader.Shader, compFacepaint.colorOne, compFacepaint.colorOne);
            compFacepaint.facepaintGraphicTwo = compFacepaint.facepaintDefTwo?.Graphic.GetColoredVersion(compFacepaint.facepaintDefTwo.shader.Shader, compFacepaint.colorTwo, compFacepaint.colorTwo);
        }
    }
}