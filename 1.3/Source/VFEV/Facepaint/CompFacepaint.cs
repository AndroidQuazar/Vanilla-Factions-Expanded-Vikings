using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEV.Facepaint
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class CompFacepaint : ThingComp
    {

        static CompFacepaint()
        {
            var thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < thingDefs.Count; i++)
            {
                var tDef = thingDefs[i];
                // Add beard comps to all eligible ThingDefs
                if (typeof(Pawn).IsAssignableFrom(tDef.thingClass) && tDef.race != null && tDef.race.Humanlike)
                {
                    if (tDef.comps == null)
                        tDef.comps = new List<CompProperties>();
                    tDef.comps.Add(new CompProperties(typeof(CompFacepaint)));
                }
            }
        }


        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.colorOne = Color.HSVToRGB(Rand.Value, 1, 1);
            this.colorTwo = Color.HSVToRGB(Rand.Value, 1, 1);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                FacepaintExtension extension = this.Pawn.kindDef.GetModExtension<FacepaintExtension>();
                if (extension != null)
                {
                    List<FacepaintDef> defs = new List<FacepaintDef>();

                    foreach (string extensionTag in extension.tags)
                        if (FacepaintDef.tagDictionary.TryGetValue(extensionTag, out List<FacepaintDef> temp))
                            defs.AddRange(temp);

                    if (defs.Any())
                        this.facepaintDefOne = defs.RandomElementByWeight(def => def.commonality);

                    defs.Clear();
                    foreach (string extensionTag in extension.tagsTwo)
                        if (FacepaintDef.tagDictionary.TryGetValue(extensionTag, out List<FacepaintDef> temp))
                            defs.AddRange(temp);

                    if (defs.Any())
                        this.facepaintDefTwo = defs.RandomElementByWeight(def => def.commonality);



                    if (extension.color != null)
                        this.colorOne = extension.color.NewRandomizedColor();
                    if (extension.colorTwo != null)
                        this.colorTwo = extension.colorTwo.NewRandomizedColor();
                }
            }
        }


        private Pawn Pawn => (Pawn)parent;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.colorOne, "colorOne");
            Scribe_Values.Look(ref this.colorTwo, "colorTwo");
            Scribe_Defs.Look(ref this.facepaintDefOne, "facepaintDefOne");
            Scribe_Defs.Look(ref this.facepaintDefTwo, "facepaintDefTwo");
        }

        public Color   colorOne;
        public Color   colorTwo;
        public FacepaintDef facepaintDefOne;
        public FacepaintDef facepaintDefTwo;

        public Graphic facepaintGraphicOne;
        public Graphic facepaintGraphicTwo;
    }
}
