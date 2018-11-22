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
    public static class ModCompatibilityCheck
    {

        public static bool JecsTools => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "JecsTools");
        public static bool AnimalVarietyCoats => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Animal Variety Coats");

    }
}
