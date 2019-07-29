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

    public static class Patch_DamageWorker_Stab
    {

        [HarmonyPatch(typeof(DamageWorker_Stab))]
        [HarmonyPatch("ChooseHitPart")]
        public static class Patch_ChooseHitPart
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

}
