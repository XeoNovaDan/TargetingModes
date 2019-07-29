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

    public static class Patch_ShotReport
    {

        [HarmonyPatch(typeof(ShotReport))]
        [HarmonyPatch(nameof(ShotReport.HitFactorFromShooter))]
        [HarmonyPatch(new Type[] { typeof(Thing), typeof(float) })]
        public static class Patch_HitFactorFromShooter
        {

            public static void Postfix(ref float __result, Thing caster)
            {
                if (caster.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
                    __result = Mathf.Max(__result * targetingComp.GetTargetingMode().HitChanceFactor, ShootTuning.MinAccuracyFactorFromShooterAndDistance);
            }

        }

    }

}
