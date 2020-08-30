using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VFEV
{
    public class MeadBarrels_MapComponent : MapComponent
    {

        //This class receives calls when a new mead barrel is built or destroyed, storing or deleting it from a List
        //This List is used on WorkGivers. They'll only look for things on the List, causing less lag

        public HashSet<Thing> meadBarrels_InMap = new HashSet<Thing>();


        public MeadBarrels_MapComponent(Map map) : base(map)
        {

        }

        public override void FinalizeInit()
        {

            base.FinalizeInit();

        }

        public void AddMeadBarrelToMap(Thing thing)
        {
            if (!meadBarrels_InMap.Contains(thing))
            {
                meadBarrels_InMap.Add(thing);
            }
        }

        public void RemoveMeadBarrelFromMap(Thing thing)
        {
            if (meadBarrels_InMap.Contains(thing))
            {
                meadBarrels_InMap.Remove(thing);
            }

        }


    }


}
