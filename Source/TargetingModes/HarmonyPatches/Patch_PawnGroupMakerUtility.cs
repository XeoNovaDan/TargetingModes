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

    public static class Patch_PawnGroupMakerUtility
    {

        [HarmonyPatch(typeof(PawnGroupMakerUtility))]
        [HarmonyPatch(nameof(PawnGroupMakerUtility.GeneratePawns))]
        public static class Patch_GeneratePawns
        {

            public static void Postfix(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
            {
                // Set targeting modes for each pawn if appropriate
                foreach (var pawn in __result)
                {
                    if ((pawn.RaceProps.Humanlike && parms.raidStrategy == TM_RaidStrategyDefOf.ImmediateAttackSmart && pawn.IsCompetentWithWeapon()) ||
                        (pawn.RaceProps.IsMechanoid && Rand.Chance(TargetingModesSettings.mechanoidTargModeChance)))
                    {
                        pawn.TryAssignRandomTargetingMode();
                    }
                }
            }

        }

    }

}
