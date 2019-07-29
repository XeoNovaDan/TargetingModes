using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace TargetingModes
{

    public static class Patch_ManhunterPackIncidentUtility
    {

        [HarmonyPatch(typeof(ManhunterPackIncidentUtility))]
        [HarmonyPatch(nameof(ManhunterPackIncidentUtility.GenerateAnimals))]
        public static class Patch_GenerateAnimals
        {

            public static void Postfix(ref List<Pawn> __result)
            {
                // Set targeting modes for each animal if appropriate
                foreach (var pawn in __result)
                {
                    if (pawn.TryGetComp<CompTargetingMode>() != null && Rand.Chance(TargetingModesUtility.AdjustedChanceForAnimal(pawn)))
                    {
                        pawn.TryAssignRandomTargetingMode();
                    }
                }
            }

        }

    }

}
