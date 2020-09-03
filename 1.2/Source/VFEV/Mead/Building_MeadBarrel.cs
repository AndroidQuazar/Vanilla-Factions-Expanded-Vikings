using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEV
{
    [StaticConstructorOnStartup]
    public class Building_MeadBarrel : Building
    {


        private int wortCount;

        private float progressInt;

        private Material barFilledCachedMat;

        public const int MaxCapacity = 75;

        private const int BaseFermentationDuration = 360000;

        public const float MinIdealTemperature = 7f;

        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

        private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);

        public override void SpawnSetup(Map map,bool respawningAfterLoad)
        {
            MeadBarrels_MapComponent mapComp = map.GetComponent<MeadBarrels_MapComponent>();
            if (mapComp != null)
            {
                mapComp.AddMeadBarrelToMap(this);
            }
            base.SpawnSetup(map, respawningAfterLoad);

        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            MeadBarrels_MapComponent mapComp = this.Map.GetComponent<MeadBarrels_MapComponent>();
            if (mapComp != null)
            {
                mapComp.RemoveMeadBarrelFromMap(this);
            }
            base.DeSpawn(mode);

        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            MeadBarrels_MapComponent mapComp = this.Map.GetComponent<MeadBarrels_MapComponent>();
            if (mapComp != null)
            {
                mapComp.RemoveMeadBarrelFromMap(this);
            }
            base.Destroy(mode);
        }


        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {
                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
                this.barFilledCachedMat = null;
            }
        }

        private Material BarFilledMat
        {
            get
            {
                if (this.barFilledCachedMat == null)
                {
                    this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Building_MeadBarrel.BarZeroProgressColor, Building_MeadBarrel.BarFermentedColor, this.Progress), false);
                }
                return this.barFilledCachedMat;
            }
        }

        public int SpaceLeftForWort
        {
            get
            {
                if (this.Fermented)
                {
                    return 0;
                }
                return MaxCapacity - this.wortCount;
            }
        }

        private bool Empty
        {
            get
            {
                return this.wortCount <= 0;
            }
        }

        public bool Fermented
        {
            get
            {
                return !this.Empty && this.Progress >= 1f;
            }
        }

        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                CompProperties_TemperatureRuinable compProperties = this.def.GetCompProperties<CompProperties_TemperatureRuinable>();
                float ambientTemperature = base.AmbientTemperature;
                if (ambientTemperature < compProperties.minSafeTemperature)
                {
                    return 0.1f;
                }
                if (ambientTemperature < 5f)
                {
                    return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature);
                }
                return 1f;
            }
        }

        private float ProgressPerTickAtCurrentTemp
        {
            get
            {
                return 2.17777781E-06f * this.CurrentTempProgressSpeedFactor;
            }
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return Mathf.Max(Mathf.RoundToInt((1f - this.Progress) / this.ProgressPerTickAtCurrentTemp), 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.wortCount, "wortCount", 0, false);
            Scribe_Values.Look<float>(ref this.progressInt, "progress", 0f, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!this.Empty)
            {
                this.Progress = Mathf.Min(this.Progress + 250f * this.ProgressPerTickAtCurrentTemp, 1f);
            }
        }

        public void AddWort(int count)
        {
            base.GetComp<CompTemperatureRuinable>().Reset();
            if (this.Fermented)
            {
                Log.Warning("Tried to add honey to a barrel full of mead. Colonists should take the mead first.", false);
                return;
            }
            int num = Mathf.Min(count, MaxCapacity - this.wortCount);
            if (num <= 0)
            {
                return;
            }
            this.Progress = GenMath.WeightedAverage(0f, (float)num, this.Progress, (float)this.wortCount);
            this.wortCount += num;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                this.Reset();
            }
        }

        private void Reset()
        {
            this.wortCount = 0;
            this.Progress = 0f;
        }

        public void AddWort(Thing wort)
        {
            int num = Mathf.Min(wort.stackCount, MaxCapacity - this.wortCount);
            if (num > 0)
            {
                this.AddWort(num);
                wort.SplitOff(num).Destroy(DestroyMode.Vanish);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
            if (!this.Empty && !comp.Ruined)
            {
                if (this.Fermented)
                {
                    stringBuilder.AppendLine("VFEV_ContainsBeer".Translate(this.wortCount, MaxCapacity));
                }
                else
                {
                    stringBuilder.AppendLine("VFEV_ContainsWort".Translate(this.wortCount, MaxCapacity));
                }
            }
            if (!this.Empty)
            {
                if (this.Fermented)
                {
                    stringBuilder.AppendLine("VFEV_Fermented".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("VFEV_FermentationProgress".Translate(this.Progress.ToStringPercent(), this.EstimatedTicksLeft.ToStringTicksToPeriod(true, false, true, true)));
                    if (this.CurrentTempProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine("VFEV_FermentationBarrelOutOfIdealTemperature".Translate(this.CurrentTempProgressSpeedFactor.ToStringPercent()));
                    }
                }
            }
            stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine("IdealFermentingTemperature".Translate() + ": " + 5f.ToStringTemperature("F0") + " ~ " + comp.Props.maxSafeTemperature.ToStringTemperature("F0"));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutMead()
        {
            if (!this.Fermented)
            {
                Log.Warning("Tried to get mead but it's not yet fermented.", false);
                return null;
            }
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("VFEV_Mead"), null);
            thing.stackCount = this.wortCount / 3;
            this.Reset();
            return thing;
        }

        public override void Draw()
        {
            base.Draw();
            if (!this.Empty)
            {
                Vector3 drawPos = this.DrawPos;
                drawPos.y += 0.042857144f;
                drawPos.z += 0.25f;
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = Building_MeadBarrel.BarSize,
                    fillPercent = (float)this.wortCount / MaxCapacity,
                    filledMat = this.BarFilledMat,
                    unfilledMat = Building_MeadBarrel.BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
           
            if (Prefs.DevMode && !this.Empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate ()
                    {
                        this.Progress = 1f;
                    }
                };
            }
            yield break;
          
        }

       
    }
}
