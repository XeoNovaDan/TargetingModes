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

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            HarmonyInstance h = HarmonyInstance.Create("XeoNovaDan.TargetingModes");

            h.Patch(AccessTools.Method(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.GeneratePawns)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_GeneratePawns)));

            h.Patch(AccessTools.Method(typeof(ShotReport), nameof(ShotReport.HitFactorFromShooter), new[] { typeof(Thing), typeof(float) }),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_HitFactorFromShooter)));

            h.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "GetNonMissChance"),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_GetNonMissChance)));

            h.Patch(AccessTools.Method(typeof(DamageWorker_AddInjury), "ChooseHitPart"),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_ChooseHitPart)));

            h.Patch(AccessTools.Method(typeof(DamageWorker_Cut), "ChooseHitPart"),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_ChooseHitPart_External)));

            h.Patch(AccessTools.Method(typeof(DamageWorker_Stab), "ChooseHitPart"),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_ChooseHitPart)));

            h.Patch(AccessTools.Method(typeof(DamageWorker_Scratch), "ChooseHitPart"),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_ChooseHitPart_External)));

            h.Patch(AccessTools.Method(typeof(DamageWorker_Blunt), "ChooseHitPart"),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_ChooseHitPart_External)));

            h.Patch(AccessTools.Method(typeof(DamageWorker_Bite), "ChooseHitPart"),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_ChooseHitPart_External)));

        }

        public static void Postfix_GeneratePawns(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
        {
            __result = ModifiedPawnGroup(__result, parms);
        }

        private static IEnumerable<Pawn> ModifiedPawnGroup(IEnumerable<Pawn> result, PawnGroupMakerParms parms)
        {
            if (result != null && result.Count() > 0)
                foreach (Pawn pawn in result)
                {
                    if (pawn.def.HasComp(typeof(CompTargetingMode)) &&
                        (pawn.RaceProps.Humanlike && parms.raidStrategy == TM_RaidStrategyDefOf.ImmediateAttackSmart && pawn.IsCompetentWithWeapon()) ||
                        (pawn.RaceProps.IsMechanoid && Rand.Chance(TargetingModesUtility.MechanoidRandomTargetingModeChance)))
                    {
                        TargetingModeDef newTargetingMode = DefDatabase<TargetingModeDef>.AllDefsListForReading.RandomElementByWeight(t => t.commonality);
                        pawn.TryGetComp<CompTargetingMode>().SetTargetingMode(newTargetingMode);
                    }
                    yield return pawn;
                }
        }

        public static void Postfix_HitFactorFromShooter(ref float __result, Thing caster)
        {
            if (caster.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
                __result = Mathf.Max(__result * targetingComp.GetTargetingMode().hitChanceFactor, ShootTuning.MinAccuracyFactorFromShooterAndDistance);
        }

        public static void Postfix_GetNonMissChance(Verb_MeleeAttack __instance, ref float __result)
        {
            if (__instance.caster is Thing caster && caster.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp && __result == caster.GetStatValue(StatDefOf.MeleeHitChance))
                __result *= __result * targetingComp.GetTargetingMode().hitChanceFactor;
        }

        public static void Postfix_ChooseHitPart(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
        {
            __result = TargetingModesUtility.ResolvePrioritizedPart(__result, dinfo, pawn);
        }

        public static void Postfix_ChooseHitPart_External(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
        {
            __result = TargetingModesUtility.ResolvePrioritizedPart_External(__result, dinfo, pawn);
        }

        public static IEnumerable<CodeInstruction> Transpile_ChooseHitPart(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            bool done = false;
            MethodInfo getRandomNotMissingPart = AccessTools.Method(typeof(HediffSet), nameof(HediffSet.GetRandomNotMissingPart));
            MethodInfo resolvePrioritizedPart = AccessTools.Method(typeof(TargetingModesUtility), nameof(TargetingModesUtility.ResolvePrioritizedPart));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done && instruction.opcode == OpCodes.Callvirt && instruction.operand == getRandomNotMissingPart)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    instruction = new CodeInstruction(OpCodes.Call, resolvePrioritizedPart);

                    done = true;
                }

                yield return instruction;
            }
        }

    }

}
