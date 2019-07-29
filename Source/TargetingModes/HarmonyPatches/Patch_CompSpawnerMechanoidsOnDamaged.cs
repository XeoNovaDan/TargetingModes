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

    public static class Patch_CompSpawnerMechanoidsOnDamaged
    {

        [HarmonyPatch(typeof(CompSpawnerMechanoidsOnDamaged))]
        [HarmonyPatch("TrySpawnMechanoids")]
        public static class Patch_TrySpawnMechanoids
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var tryAssignRandomTargetingModeMechanoid = AccessTools.Method(typeof(Patch_TrySpawnMechanoids), nameof(TryAssignRandomTargetingModeMechanoid));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Something oddly satisfying about this if-statement
                    // Basically "if line == this.pointsLeft -= pawn.kindDef.combatPower;"
                    if (IsTheInstructionIAmLookingFor(instruction, instructionList, i))
                    {
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
                        instruction = new CodeInstruction(OpCodes.Call, tryAssignRandomTargetingModeMechanoid);
                    }

                    yield return instruction;
                }
            }

            // I'm going mad
            private static bool IsTheInstructionIAmLookingFor(CodeInstruction instruction, List<CodeInstruction> instructionList, int i)
            {
                // If the transpiler iterator's at the beginning of the method, which isn't where the relevant code is anyway
                if (i - 2 < 0)
                    return false;

                var prevInst = instructionList[(i - 1)];
                var prevInst2 = instructionList[(i - 2)];

                return
                    instruction.opcode == OpCodes.Stfld && instruction.operand == AccessTools.Field(typeof(CompSpawnerMechanoidsOnDamaged), nameof(CompSpawnerMechanoidsOnDamaged.pointsLeft)) &&
                    prevInst.opcode == OpCodes.Sub &&
                    prevInst2.opcode == OpCodes.Ldfld && prevInst2.operand == AccessTools.Field(typeof(PawnKindDef), nameof(PawnKindDef.combatPower));
            }


            private static void TryAssignRandomTargetingModeMechanoid(Pawn pawn)
            {
                if (Rand.Chance(TargetingModesSettings.mechanoidTargModeChance))
                    pawn.TryAssignRandomTargetingMode();
            }

        }

    }

}
