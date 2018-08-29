using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace TargetingModes
{

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            HarmonyInstance h = HarmonyInstance.Create("XeoNovaDan.TargetingModes");

            h.Patch(AccessTools.Method(typeof(DamageWorker_AddInjury), "ChooseHitPart"), null,
                new HarmonyMethod(patchType, nameof(PostfixChooseHitPart)));

        }

        public static void PostfixChooseHitPart(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
        {
            if (pawn.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
            {
                TargetingModeDef targetingMode = targetingComp.GetTargetingMode();
                if ((!targetingMode.parts.NullOrEmpty() && !targetingMode.parts.Contains(__result.def)) ||
                    (!targetingMode.tags.NullOrEmpty() && !TargetingModeTagsContains(targetingMode, __result)))
                    __result = RerollBodyPart(__result, dinfo, pawn);
            }
        }

        public static bool TargetingModeTagsContains(TargetingModeDef targetingMode, BodyPartRecord bodyPart)
        {
            if (!targetingMode.tags.NullOrEmpty())
            {
                foreach (BodyPartTagDef tag in bodyPart.def.tags)
                {
                    if (targetingMode.tags.Contains(tag))
                        return true;
                }
                return false;
            }
            return true;
        }

        public static BodyPartRecord RerollBodyPart(BodyPartRecord bodyPart, DamageInfo dinfo, Pawn pawn)
        {
            BodyPartRecord newPart = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
            return (newPart != bodyPart) ? newPart : bodyPart;
        }

    }

}
