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
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair"),
                          transpiler: new HarmonyMethod(typeof(HarmonyPatches_Facepaint), methodName: nameof(DrawHeadHairTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnGraphicSet), name: nameof(PawnGraphicSet.ResolveAllGraphics)),
                          postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches_Facepaint), methodName: nameof(ResolveAllGraphicsPostfix)));
        }

        public static IEnumerable<CodeInstruction> DrawHeadHairTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionList = instructions.ToList();
            bool hideHairDeclared = false;
            bool hideHairAssigned = false;
            bool drawFacepaintCall = false;

            var hideFacepaint = generator.DeclareLocal(typeof(bool));

            var getItemInfo = AccessTools.Method(typeof(List<ApparelGraphicRecord>),"get_Item");

            var getHatsOnlyOnMapInfo = AccessTools.Property(typeof(Prefs), nameof(Prefs.HatsOnlyOnMap)).GetGetMethod();
            var ideologyActiveInfo = AccessTools.Property(typeof(ModsConfig), nameof(ModsConfig.IdeologyActive)).GetGetMethod();

            var shouldHideHeadgearInfo = AccessTools.Method(typeof(HarmonyPatches_Facepaint), nameof(ShouldHideHeadgear));
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
                if (instruction.opcode == OpCodes.Stloc_3)
                {
                    if (!hideHairDeclared)
                    {
                        yield return instruction;                                                          // bool beardHidden
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);                                // false
                        instruction      = new CodeInstruction(OpCodes.Stloc_S, hideFacepaint.LocalIndex); // hideFacepaint = false
                        hideHairDeclared = true;
                    }
                    else if (!hideHairAssigned)
                    {
                        yield return instruction; // flag = true
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        instruction = new CodeInstruction(OpCodes.Stloc_S, hideFacepaint.LocalIndex); // hideFacepaint
                        hideHairAssigned = true;
                    }
                }
                else if (!drawFacepaintCall && hideHairAssigned && instruction.Calls(ideologyActiveInfo))// && instruction.operand is LocalBuilder lb && lb.LocalIndex == 4)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) {labels = instruction.ExtractLabels()}; // this
                    yield return new CodeInstruction(OpCodes.Ldloc_S, hideFacepaint.LocalIndex); // hideFacepaint
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6); // bodyDrawType
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7); // renderFlags
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5); // headFacing
                    //yield return new CodeInstruction(OpCodes.Ldloc_S, 7); // loc2
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // display class
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer).GetNestedTypes(AccessTools.all)[0], "onHeadLoc")); // onHeadLoc
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // display class
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer).GetNestedTypes(AccessTools.all)[0], "quat")); // quaternion
                    yield return new CodeInstruction(OpCodes.Call,  renderFacepaint); // RenderFacepaint(this, hideFacepaint, bodyDrawType, renderFlags, headFacing, loc2, quaternion)
                    drawFacepaintCall = true;
                }


                yield return instruction;
            }
        }

        private static bool ShouldHideHeadgear(bool original)
        {
            // Also check if there are any change hairstyle windows currently open
            return original || Find.WindowStack.Windows.Any(w => w is Dialog_ChangeFacepaint);
        }

        private static void RenderFacepaint(PawnRenderer instance, bool hideFacepaint, RotDrawMode bodyDrawType, PawnRenderFlags flags, Rot4 headFacing, Vector3 baseDrawPos, Quaternion quaternion)
        {
            var pawn = instance.graphics.pawn;
            if (bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump) &&
                pawn.GetComp<CompFacepaint>() is CompFacepaint facepaintComp)
            {
                Mesh mesh = instance.graphics.HairMeshSet.MeshAt(headFacing);

                if (facepaintComp.facepaintGraphicOne != null)
                    GenDraw.DrawMeshNowOrLater(mesh, baseDrawPos - new Vector3(0, 0.0007f, 0), quaternion, facepaintComp.facepaintGraphicOne.MatAt(headFacing), flags.FlagSet(PawnRenderFlags.DrawNow));

                if (facepaintComp.facepaintGraphicTwo != null)
                    GenDraw.DrawMeshNowOrLater(mesh, baseDrawPos - new Vector3(0, 0.0005f, 0), quaternion, facepaintComp.facepaintGraphicTwo.MatAt(headFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
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