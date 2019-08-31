using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{

    public static class ModCompatibilityCheck
    {

        public static bool CombatTweaks => ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[XND] Combat Tweaks");

    }

}
