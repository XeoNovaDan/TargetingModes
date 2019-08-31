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

    public static class Patch_CombatTweaks_CombatTweaksUtility
    {

        public static class ManualPatch_AccuracyScore_AimOnTargetChance_StandardTarget
        {

            public static void Postfix(ShotReport report, ref float __result)
            {
                var traverse = Traverse.Create(report);
                var pawn = traverse.Field("");
            }

        }

    }

}
