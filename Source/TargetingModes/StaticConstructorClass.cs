using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                // Add CompTargetingMode to all pawn defs
                if (typeof(Pawn).IsAssignableFrom(tDef.thingClass))
                {
                    if (tDef.comps.NullOrEmpty())
                        tDef.comps = new List<CompProperties>();
                    tDef.comps.Add(new CompProperties(typeof(CompTargetingMode)));
                }
            }
        }

    }

}
