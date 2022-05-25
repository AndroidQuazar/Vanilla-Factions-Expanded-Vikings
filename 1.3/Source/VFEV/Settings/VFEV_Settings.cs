using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;


namespace VFEV
{
    public class VFEV_Settings : ModSettings

    {



        public const float VFEV_HuntQuestMultiplierBase = 1;
        public float VFEV_HuntQuestMultiplier = VFEV_HuntQuestMultiplierBase;

        public const float VFEV_StructureQuestMultiplierBase = 1;
        public float VFEV_StructureQuestMultiplier = VFEV_StructureQuestMultiplierBase;

      

        private static Vector2 scrollPosition = Vector2.zero;





        public override void ExposeData()
        {
            base.ExposeData();


            Scribe_Values.Look(ref VFEV_HuntQuestMultiplier, "VFEV_HuntQuestMultiplier", VFEV_HuntQuestMultiplierBase);
            Scribe_Values.Look(ref VFEV_StructureQuestMultiplier, "VFEV_StructureQuestMultiplier", VFEV_StructureQuestMultiplierBase);
          






        }
        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            var scrollContainer = inRect.ContractedBy(10);
            scrollContainer.height -= listingStandard.CurHeight;
            scrollContainer.y += listingStandard.CurHeight;
            Widgets.DrawBoxSolid(scrollContainer, Color.grey);
            var innerContainer = scrollContainer.ContractedBy(1);
            Widgets.DrawBoxSolid(innerContainer, new ColorInt(42, 43, 44).ToColor);
            var frameRect = innerContainer.ContractedBy(5);
            frameRect.y += 15;
            frameRect.height -= 15;
            var contentRect = frameRect;
            contentRect.x = 0;
            contentRect.y = 0;
            contentRect.width -= 20;




            contentRect.height = 350f;


            Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect, true);
            listingStandard.Begin(contentRect.AtZero());


           


            listingStandard.GapLine();
        

            var huntQuestRateLabel = listingStandard.LabelPlusButton("VFEV_HuntQuestMultiplier".Translate() + ": " + VFEV_HuntQuestMultiplier, "VFEV_HuntQuestMultiplierTooltip".Translate());
            VFEV_HuntQuestMultiplier = (float)Math.Round(listingStandard.Slider(VFEV_HuntQuestMultiplier, 0.1f, 5f), 1);
            if (listingStandard.Settings_Button("VFEV_Reset".Translate(), new Rect(0f, huntQuestRateLabel.position.y + 35, 180f, 29f)))
            {
                VFEV_HuntQuestMultiplier = VFEV_HuntQuestMultiplierBase;
            }

            var structureQuestRateLabel = listingStandard.LabelPlusButton("VFEV_StructureQuestMultiplier".Translate() + ": " + VFEV_StructureQuestMultiplier, "VFEV_StructureQuestMultiplierTooltip".Translate());
            VFEV_StructureQuestMultiplier = (float)Math.Round(listingStandard.Slider(VFEV_StructureQuestMultiplier, 0.1f, 5f), 1);
            if (listingStandard.Settings_Button("VFEV_Reset".Translate(), new Rect(0f, structureQuestRateLabel.position.y + 35, 180f, 29f)))
            {
                VFEV_StructureQuestMultiplier = VFEV_StructureQuestMultiplierBase;
            }

          

            listingStandard.End();
            Widgets.EndScrollView();

            base.Write();

        }




    }










}
