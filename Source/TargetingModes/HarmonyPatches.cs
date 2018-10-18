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

            #region Self-Patches
            try
            {
                ((Action)(() =>
                {
                    if (ModCompatibilityCheck.JecsTools)
                    {
                        Log.Message("Targeting Modes :: JecsTools detected as active in load order. Patching...");

                        h.Patch(AccessTools.Method(typeof(TargetingModesUtility), nameof(TargetingModesUtility.CanUseTargetingModes)),
                            new HarmonyMethod(patchType, nameof(Prefix_CanUseTargetingModes)),
                            new HarmonyMethod(patchType, nameof(Postfix_CanUseTargetingModes)));
                    }
                }))();
            }
            catch (TypeLoadException) { }
            #endregion

            #region Patches
            h.Patch(AccessTools.Method(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.GeneratePawns)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_GeneratePawns)));

            h.Patch(AccessTools.Method(typeof(ManhunterPackIncidentUtility), nameof(ManhunterPackIncidentUtility.GenerateAnimals)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_GenerateAnimals)));

            h.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_GetGizmos)));

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

            h.Patch(AccessTools.Method(typeof(CompSpawnerMechanoidsOnDamaged), "TrySpawnMechanoids"),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_TrySpawnMechanoids)));
            #endregion

        }

        #region Patch_CanUseTargetingModes
        public static bool Prefix_CanUseTargetingModes(ThingDef weapon, bool? __state)
        {
            if (weapon?.GetType().IsAssignableFrom(typeof(AbilityUser.ProjectileDef_Ability)) == true)
            {
                __state = false;
                return false;
            }
            return true;
        }

        public static void Postfix_CanUseTargetingModes(ref bool __result, bool? __state)
        {
            if (__state != null)
                __result = (bool)__state;
        }
        #endregion

        #region Postfix_GeneratePawns
        public static void Postfix_GeneratePawns(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
        {
            __result = ModifiedPawnGroup(__result, parms);
        }

        private static IEnumerable<Pawn> ModifiedPawnGroup(IEnumerable<Pawn> result, PawnGroupMakerParms parms)
        {
            if (result != null && result.Count() > 0)
                foreach (Pawn pawn in result)
                {
                    if ((pawn.RaceProps.Humanlike && parms.raidStrategy == TM_RaidStrategyDefOf.ImmediateAttackSmart && pawn.IsCompetentWithWeapon()) ||
                        (pawn.RaceProps.IsMechanoid && Rand.Chance(TargetingModesSettings.mechanoidTargModeChance)))
                    {
                        // Validation for CompTargetingMode is done within this method
                        pawn.TryAssignRandomTargetingMode();
                    }
                    yield return pawn;
                }
        }
        #endregion

        #region Postfix_GenerateAnimals
        public static void Postfix_GenerateAnimals(ref List<Pawn> __result)
        {
            __result = ModifiedAnimalGroup(__result).ToList();
        }

        private static IEnumerable<Pawn> ModifiedAnimalGroup(List<Pawn> result)
        {
            if (!result.NullOrEmpty())
                foreach (Pawn pawn in result)
                {
                    if (Rand.Chance(TargetingModesUtility.AdjustedChanceForAnimal(pawn)))
                    {
                        pawn.TryAssignRandomTargetingMode();
                    }
                    yield return pawn;
                }
        }
        #endregion

        #region Postfix_GetGizmos
        public static void Postfix_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = ModifiedGizmoEnumerable(__instance, __result);
        }

        private static IEnumerable<Gizmo> ModifiedGizmoEnumerable(Pawn pawn, IEnumerable<Gizmo> result)
        {
            foreach (Gizmo gizmo in result)
                yield return gizmo;

            if (pawn.IsPlayerControlledAnimal() && pawn.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
                foreach (Gizmo gizmo in targetingComp.CompGetGizmosExtra())
                    yield return gizmo;
        }

        private static bool IsPlayerControlledAnimal(this Pawn pawn) =>
            pawn.Spawned && pawn.MentalStateDef == null && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer;
        #endregion

        #region AccuracyModifyingPatches
        public static void Postfix_HitFactorFromShooter(ref float __result, Thing caster)
        {
            if (caster.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
                __result = Mathf.Max(__result * targetingComp.GetTargetingMode().HitChanceFactor, ShootTuning.MinAccuracyFactorFromShooterAndDistance);
        }

        public static void Postfix_GetNonMissChance(Verb_MeleeAttack __instance, ref float __result)
        {
            if (__instance.caster is Thing caster && caster.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp && __result == caster.GetStatValue(StatDefOf.MeleeHitChance))
                __result *= __result * targetingComp.GetTargetingMode().HitChanceFactor;
        }
        #endregion

        #region DamageWorkerModifyingPatches
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
        #endregion

        #region Transpile_TrySpawnMechanoids
        public static IEnumerable<CodeInstruction> Transpile_TrySpawnMechanoids(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo tryAssignRandomTargetingModeMechanoid = AccessTools.Method(patchType, nameof(TryAssignRandomTargetingModeMechanoid));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                // Something oddly satisfying about this if-statement
                // Basically "if line == this.pointsLeft -= pawn.kindDef.combatPower;"
                if (instruction.IsTheInstructionIAmLookingFor(instructionList, i))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
                    instruction = new CodeInstruction(OpCodes.Call, tryAssignRandomTargetingModeMechanoid);
                }

                yield return instruction;
            }
        }

        // I'm going mad
        private static bool IsTheInstructionIAmLookingFor(this CodeInstruction instruction, List<CodeInstruction> instructionList, int i)
        {
            // If the transpiler iterator's at the beginning of the method, which isn't where the relevant code is anyway
            if (i - 2 < 0)
                return false;

            CodeInstruction prevInst = instructionList[(i - 1)];
            CodeInstruction prevInst2 = instructionList[(i - 2)];

            return
                instruction.opcode == OpCodes.Stfld && instruction.operand == AccessTools.Field(typeof(CompSpawnerMechanoidsOnDamaged), nameof(CompSpawnerMechanoidsOnDamaged.pointsLeft)) &&
                prevInst.opcode == OpCodes.Sub &&
                prevInst2.opcode == OpCodes.Ldfld && prevInst2.operand == AccessTools.Field(typeof(PawnKindDef), nameof(PawnKindDef.combatPower));
        }


        private static void TryAssignRandomTargetingModeMechanoid(this Pawn pawn)
        {
            if (Rand.Chance(TargetingModesSettings.mechanoidTargModeChance))
                pawn.TryAssignRandomTargetingMode();
        }
        #endregion

    }

}
