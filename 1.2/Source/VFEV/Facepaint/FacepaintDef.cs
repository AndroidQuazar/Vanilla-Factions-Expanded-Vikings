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

    public class FacepaintDef : Def
    {
        public static Dictionary<string, List<FacepaintDef>> tagDictionary = new Dictionary<string, List<FacepaintDef>>();

        public string texPath = string.Empty;
        public List<string> tags;
        public ShaderTypeDef shader;
        private Graphic graphic = null;
        public int  workToStyle = 300;
        public float commonality = 10f;

        public Graphic Graphic => 
            this.graphic ?? (this.graphic = GraphicDatabase.Get<Graphic_Multi>(this.texPath, shader.Shader, Vector2.one, Color.white));

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors()) 
                yield return configError;

            if (this.texPath.NullOrEmpty())
                yield return "missing texture path";

            if (this.shader == null)
                yield return "missing shader";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (this.shader == null) 
                this.shader = ShaderTypeDefOf.Cutout;

            foreach (string tag in this.tags)
            {
                if(!tagDictionary.ContainsKey(tag))
                    tagDictionary.Add(tag, new List<FacepaintDef>());
                tagDictionary[tag].Add(this);
            }
        }
    }
}
