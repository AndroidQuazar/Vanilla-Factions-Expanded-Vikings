using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEV.Facepaint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Verse.Sound;
    using RimWorld;

    [StaticConstructorOnStartup]
    public class Dialog_ChangeFacepaint : Window
    {

        public Dialog_ChangeFacepaint(JobDriver_ChangeFacepaint jobDriver)
        {
            forcePause              = true;
            doCloseX                = true;
            absorbInputAroundWindow = true;
            closeOnAccept           = true;
            closeOnClickedOutside   = true;

            this.jobDriver          = jobDriver;
            this.initFacepaintCombo = new FacepaintCombination(Pawn);
            this.newFacepaintCombo  = this.initFacepaintCombo;

            coloursTied = this.initFacepaintCombo.colourOne.IndistinguishableFrom(this.initFacepaintCombo.colourTwo);
        }

        public override Vector2 InitialSize => new Vector2(900, 700);

        public override void WindowOnGUI()
        {
            // Update preview
            this.newFacepaintCombo.ApplyToPawn(Pawn, coloursTied);
            base.WindowOnGUI();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font   = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Check for a press of 'Return' and process as a confirmation if so
            bool pressedReturn = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                pressedReturn = true;
                Event.current.Use();
            }

            // Preview
            var    fullPawnConfirmationRect = inRect.LeftPart(0.4f).ContractedBy(12);
            var    pawnLabelRect            = fullPawnConfirmationRect.TopPart(0.08f);
            string pawnLabel                = Pawn.Label;
            while (Text.CalcSize(pawnLabel).x > pawnLabelRect.width && Text.Font > GameFont.Tiny)
                Text.Font--;
            pawnLabel = Text.CalcSize(pawnLabel).x <= pawnLabelRect.width ? pawnLabel : pawnLabel.Truncate(pawnLabelRect.width, truncatedLabelCache);
            Widgets.Label(pawnLabelRect, Pawn.Label);

            var pawnConfirmationRect = fullPawnConfirmationRect.BottomPart(0.92f);
            var previewRect = pawnConfirmationRect.TopPartPixels(pawnConfirmationRect.width);
            GUI.DrawTexture(previewRect, ColonistBar.BGTex);
            previewRect = previewRect.ContractedBy(6);
            GUI.DrawTexture(previewRect, PortraitsCache.Get(Pawn, previewRect.size));

            var fullOptionRectsArea = inRect.RightPart(0.6f);
            // Hair/beard options
            var optionRectsArea = fullOptionRectsArea.TopPart(0.75f);

            var hairRect        = optionRectsArea.LeftHalf().ContractedBy(12);
            var hairHeadingRect = hairRect.TopPart(0.08f);
            Widgets.Label(hairHeadingRect, "VanillaFactionsExpandedVikings.Facepaint".Translate());
            var hairListRect = hairRect.BottomPart(0.92f);
            Widgets.DrawMenuSection(hairListRect);
            hairListRect = hairListRect.ContractedBy(6);
            var firstListing  = new Listing_Standard();
            var hairViewRect = new Rect(0, 0, hairListRect.width - 18, hairViewRectHeight);
            var beardRect        = optionRectsArea.RightHalf().ContractedBy(12);
            var beardHeadingRect = beardRect.TopPart(0.08f);
            Widgets.Label(beardHeadingRect, "VanillaFactionsExpandedVikings.Facepaint".Translate());
            var beardListRect = beardRect.BottomPart(0.92f);
            Widgets.DrawMenuSection(beardListRect);
            beardListRect = beardListRect.ContractedBy(6);
            var secondListing  = new Listing_Standard();
            var beardViewRect = new Rect(0, 0, beardListRect.width - 18, beardViewRectHeight);

            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            CompFacepaint compFacepaint = Pawn.GetComp<CompFacepaint>();

            firstListing.BeginScrollView(hairListRect, ref hairScrollVec2, ref hairViewRect);
            for (int i = 0; i < orderedFacepaintDefs.Count; i++)
            {
                var hDef = orderedFacepaintDefs[i];
                DrawRow(firstListing, hDef, compFacepaint.colorOne, ref this.newFacepaintCombo.facepaintDefOne);
            }
            firstListing.EndScrollView(ref hairViewRect);

            secondListing.BeginScrollView(beardListRect, ref beardScrollVec2, ref beardViewRect);
            for (int i = 0; i < orderedFacepaintDefs.Count; i++)
            {
                var hDef = orderedFacepaintDefs[i];
                DrawRow(secondListing, hDef, compFacepaint.colorTwo, ref this.newFacepaintCombo.facepaintDefTwo);
            }

            secondListing.EndScrollView(ref beardViewRect);

            hairViewRectHeight  = firstListing.CurHeight;
            beardViewRectHeight = secondListing.CurHeight;

            // Colour selection
            var fullColourRectsArea = fullOptionRectsArea.BottomPart(0.25f).ContractedBy(12);
            var colourRectsArea     = fullColourRectsArea.TopPart(0.7f);
            if (coloursTied)
            {
                DrawColourChangeSection(colourRectsArea, ref this.newFacepaintCombo.colourOne);
            }
            else
            {
                DrawColourChangeSection(colourRectsArea.LeftPartPixels(colourRectsArea.width  / 2 - 12), ref this.newFacepaintCombo.colourOne);
                DrawColourChangeSection(colourRectsArea.RightPartPixels(colourRectsArea.width / 2 - 12), ref this.newFacepaintCombo.colourTwo);
            }

            var fullCheckboxRectsArea = fullColourRectsArea.BottomPart(0.2f);

            var customCheckboxRect = fullCheckboxRectsArea.LeftPart(0.45f);
            Widgets.CheckboxLabeled(customCheckboxRect, "VanillaFactionsExpandedVikings.CustomMode".Translate(), ref colourSliders);

            var tiedCheckboxRect = fullCheckboxRectsArea.RightPart(0.45f);
            Widgets.CheckboxLabeled(tiedCheckboxRect, "VanillaFactionsExpandedVikings.ColoursTied".Translate(), ref coloursTied);

            // Work amount, Reset and Confirm
            var workConfirmRect = pawnConfirmationRect.BottomPart(0.2f).LeftPart(0.9f).RightPart(8f / 9);

            var workRect = workConfirmRect.TopHalf();
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(workRect, $"{"WorkAmount".Translate()}: {((float) RestyleTicks).ToStringWorkAmount()}");
            Text.Anchor = TextAnchor.UpperLeft;

            var resetConfirmRect = workConfirmRect.BottomHalf();

            if (Widgets.ButtonText(resetConfirmRect.LeftPart(0.48f), "Reset".Translate()))
            {
                this.newFacepaintCombo = this.initFacepaintCombo;
                RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
            }

            if (Widgets.ButtonText(resetConfirmRect.RightPart(0.48f), "Confirm".Translate()) || pressedReturn)
            {
                SetHairstyle();
                Find.WindowStack.TryRemove(this);
            }
        }

        private void DrawRow(Listing_Standard usedListing, FacepaintDef listedFacepaint, Color colour, ref FacepaintDef hairToChange)
        {
            const int rowHeight      = 72;
            var       originalColour = GUI.color;
            var       rect           = usedListing.GetRect(rowHeight);

            // Full-rect stuff
            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);
            else if (listedFacepaint == hairToChange)
            {
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a * 0.5f);
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                GUI.color = originalColour;
            }

            if (Widgets.ButtonInvisible(rect, true))
            {
                hairToChange = hairToChange == listedFacepaint ? null : listedFacepaint;
                RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
            }

            var previewImageRect = rect.LeftPartPixels(rowHeight);
            var hairGraphic      = listedFacepaint.Graphic.GetColoredVersion(listedFacepaint.shader.Shader, colour, colour);
            GUI.color = colour;
            GUI.DrawTexture(previewImageRect, hairGraphic.MatSouth.mainTexture);
            GUI.color = originalColour;

            // Text
            var textRect       = rect.RightPartPixels(rect.width - rowHeight - usedListing.verticalSpacing);
            var originalFont   = Text.Font;
            var originalAnchor = Text.Anchor;
            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.LowerLeft;
            string hairLabelCap = listedFacepaint.LabelCap;
            if (Text.CalcSize(hairLabelCap).x > textRect.width)
            {
                string hairLabelCapOriginal = hairLabelCap;
                hairLabelCap = hairLabelCap.Truncate(textRect.width, truncatedLabelCache);
                TooltipHandler.TipRegion(textRect, () => hairLabelCapOriginal, listedFacepaint.GetHashCode());
            }

            Widgets.Label(textRect.TopHalf(), hairLabelCap);
            Text.Font   = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(textRect.BottomHalf(), $"{"WorkAmount".Translate()}: {((float) listedFacepaint.workToStyle).ToStringWorkAmount()}");
            Text.Font   = originalFont;
            Text.Anchor = originalAnchor;

            usedListing.Gap(usedListing.verticalSpacing);
        }

        private void DrawColourChangeSection(Rect rect, ref Color colour)
        {
            Widgets.DrawMenuSection(rect);
            var filledRect = rect.ScaledBy(0.9f);

            // Slider (custom mode)
            if (colourSliders)
            {
                filledRect.y += filledRect.height / 6;
                var   rRect = new Rect(filledRect.x, filledRect.y,                             filledRect.width, filledRect.height                                   / 3);
                var   gRect = new Rect(filledRect.x, filledRect.y + filledRect.height     / 3, filledRect.width, filledRect.height                                   / 3);
                var   bRect = new Rect(filledRect.x, filledRect.y + filledRect.height * 2 / 3, filledRect.width, filledRect.height                                   / 3);
                float r     = Widgets.HorizontalSlider(rRect, colour.r, 0, 1, leftAlignedLabel: "R", rightAlignedLabel: (colour.r * 255).ToString("F0"), roundTo: 1f / 255);
                float g     = Widgets.HorizontalSlider(gRect, colour.g, 0, 1, leftAlignedLabel: "G", rightAlignedLabel: (colour.g * 255).ToString("F0"), roundTo: 1f / 255);
                float b     = Widgets.HorizontalSlider(bRect, colour.b, 0, 1, leftAlignedLabel: "B", rightAlignedLabel: (colour.b * 255).ToString("F0"), roundTo: 1f / 255);
                colour = new Color(r, g, b);
            }

            // Presets
            else
            {
                const int presetsPerRow = 6;
                var       row           = filledRect.TopPartPixels(24);
                var       originalFont  = Text.Font;
                Text.Font = GameFont.Tiny;
                for (int i = 0; i < DefDatabase<ColourPresetDef>.AllDefsListForReading.Count; i++)
                {
                    // New row
                    int positionsAcross = i % presetsPerRow;
                    if (i > 0 && positionsAcross == 0)
                        row.y += row.height + 6;

                    // Colour preview
                    var   curColour  = DefDatabase<ColourPresetDef>.AllDefsListForReading[i];
                    var   colourRect = new Rect(row.x + (float) positionsAcross / presetsPerRow * row.width, row.y, row.width / presetsPerRow, row.height);
                    float margin     = (colourRect.width - 24) / 2;
                    colourRect = colourRect.LeftPartPixels(colourRect.width - margin).RightPartPixels(colourRect.width - margin);
                    var originalGuiCol = GUI.color;
                    GUI.color = curColour.colour;
                    GUI.DrawTexture(colourRect, BlankColourTex);
                    GUI.color = originalGuiCol;
                    if (Widgets.ButtonInvisible(colourRect))
                        colour = curColour.colour;
                }

                Text.Font = originalFont;
            }
        }

        private void SetHairstyle()
        {
            // Hair def
            jobDriver.newFacepaintDefOne = this.newFacepaintCombo.facepaintDefOne;

            // Hair colour
            if (!this.newFacepaintCombo.colourOne.IndistinguishableFrom(this.initFacepaintCombo.colourOne))
                jobDriver.newFacepaintColorOne = this.newFacepaintCombo.colourOne;

            // Beard def
            jobDriver.newFacepaintDefTwo = this.newFacepaintCombo.facepaintDefTwo;

            // Beard colour
            if (!this.newFacepaintCombo.FacepaintColor(coloursTied).IndistinguishableFrom(this.initFacepaintCombo.FacepaintColor(coloursTied)))
                jobDriver.newFacepaintColorTwo = this.newFacepaintCombo.FacepaintColor(coloursTied);

            jobDriver.ticksToRestyle = RestyleTicks;
        }

        public override void PostClose()
        {
            base.PostClose();
            this.initFacepaintCombo.ApplyToPawn(Pawn, coloursTied);
            jobDriver.ReadyForNextToil();
        }

        private int RestyleTicks
        {
            get
            {
                int restyleTicks = 0;

                if (this.newFacepaintCombo.facepaintDefOne != this.initFacepaintCombo.facepaintDefOne)
                    restyleTicks += this.newFacepaintCombo.facepaintDefOne?.workToStyle ?? ShaveWorkTicks;

                if (!this.newFacepaintCombo.colourOne.IndistinguishableFrom(this.initFacepaintCombo.colourOne))
                    restyleTicks += RecolourWorkTicks;

                if (this.newFacepaintCombo.facepaintDefTwo != this.initFacepaintCombo.facepaintDefTwo)
                    restyleTicks += this.newFacepaintCombo.facepaintDefTwo?.workToStyle ?? ShaveWorkTicks;

                if (!this.newFacepaintCombo.FacepaintColor(coloursTied).IndistinguishableFrom(this.initFacepaintCombo.FacepaintColor(coloursTied)))
                    restyleTicks += RecolourWorkTicks;

                return restyleTicks;
            }
        }

        private const           int       RecolourWorkTicks = 300;
        private const int ShaveWorkTicks = 150;
        private static readonly Texture2D BlankColourTex    = ContentFinder<Texture2D>.Get("UI/Misc/BlankUICol");

        private FacepaintCombination      initFacepaintCombo;
        private FacepaintCombination      newFacepaintCombo;
        private JobDriver_ChangeFacepaint jobDriver;

        private static bool coloursTied = true;
        private static bool colourSliders;

        private Pawn Pawn => jobDriver.pawn;

        private Vector2 hairScrollVec2;
        private float   hairViewRectHeight;
        private Vector2 beardScrollVec2;
        private float   beardViewRectHeight;

        private static readonly List<FacepaintDef>         orderedFacepaintDefs = DefDatabase<FacepaintDef>.AllDefs.OrderBy(h => h.LabelCap.RawText).ToList();
        private static readonly Dictionary<string, string> truncatedLabelCache  = new Dictionary<string, string>();

        private struct FacepaintCombination
        {

            public FacepaintCombination(Pawn pawn)
            {
                var facepaintComp = pawn.GetComp<CompFacepaint>();
                this.facepaintDefOne = facepaintComp?.facepaintDefOne;
                this.facepaintDefTwo = facepaintComp?.facepaintDefTwo;
                this.colourOne       = facepaintComp?.colorOne ?? default;
                this.colourTwo       = facepaintComp?.colorTwo ?? default;
            }

            public void ApplyToPawn(Pawn pawn, bool coloursTied)
            {

                if (pawn.GetComp<CompFacepaint>() is CompFacepaint facepaintComp)
                {

                    facepaintComp.facepaintDefOne = this.facepaintDefOne;
                    facepaintComp.colorOne        = this.colourOne;
                    facepaintComp.facepaintDefTwo = this.facepaintDefTwo;
                    facepaintComp.colorTwo        = this.FacepaintColor(coloursTied);
                }

                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawn);
            }

            public Color FacepaintColor(bool coloursTied)
            {
                if (coloursTied)
                    this.colourTwo = this.colourOne;
                return this.colourTwo;
            }

            public FacepaintDef facepaintDefOne;
            public Color        colourOne;
            public FacepaintDef facepaintDefTwo;
            public Color        colourTwo;

        }

    }

}