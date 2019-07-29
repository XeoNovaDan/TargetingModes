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

    public static class Patch_Verb_MeleeAttack
    {

        [HarmonyPatch(typeof(Verb_MeleeAttack))]
        [HarmonyPatch("GetNonMissChance")]
        public static class Patch_GetNonMissChance
        {

            public static void Postfix(Verb_MeleeAttack __instance, ref float __result)
            {
                if (__instance.caster is Thing caster && caster.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp && __result == caster.GetStatValue(StatDefOf.MeleeHitChance))
                    __result *= __result * targetingComp.GetTargetingMode().HitChanceFactor;
            }

        }

    }

}
