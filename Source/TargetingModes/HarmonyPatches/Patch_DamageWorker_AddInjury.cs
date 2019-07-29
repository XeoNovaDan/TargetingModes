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

    public static class Patch_DamageWorker_AddInjury
    {

        [HarmonyPatch(typeof(DamageWorker_AddInjury))]
        [HarmonyPatch("ChooseHitPart")]
        public static class Patch_ChooseHitPart
        {

            public static void Postfix(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
            {
                __result = TargetingModesUtility.ResolvePrioritizedPart(__result, dinfo, pawn);
            }

        }

    }

}
