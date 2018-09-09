using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{
    public static class TargetingModesUtility
    {

        public static Command_SetTargetingMode SetTargetModeCommand(ITargetModeSettable settable)
        {
            return new Command_SetTargetingMode
            {
                defaultDesc = "CommandSetTargetingModeDesc".Translate(),
                settable = settable
            };
        }
        
        // Only exists to make transpiling much less of a pain
        public static BodyPartRecord ResolvePrioritizedPart(BodyPartRecord part, DamageInfo dinfo, Pawn pawn)
        {
            BodyPartRecord newPart = part;
            if (dinfo.Instigator?.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
            {
                TargetingModeDef targetingMode = targetingComp.GetTargetingMode();
                if (!part.IsPrioritizedPart(targetingMode))
                    newPart = RerollBodyPart(targetingMode, part, dinfo, pawn);
            }
            return newPart;
        }

        public static BodyPartRecord ResolvePrioritizedPart_External(BodyPartRecord part, DamageInfo dinfo, Pawn pawn)
        {
            BodyPartRecord newPart = part;
            if (dinfo.Instigator?.TryGetComp<CompTargetingMode>() is CompTargetingMode targetingComp)
            {
                TargetingModeDef targetingMode = targetingComp.GetTargetingMode();
                if (!part.IsPrioritizedPart(targetingMode))
                    newPart = RerollBodyPart(targetingMode, part, dinfo.Def, dinfo.Height, BodyPartDepth.Outside, pawn);
            }
            return newPart;
        }

        public static bool IsPrioritizedPart(this BodyPartRecord part, TargetingModeDef targetingMode) =>
            targetingMode.HasNoSpecifiedPartDetails ||
            targetingMode.PartsListContains(part.def) ||
            targetingMode.PartsOrAnyChildrenListContains(part) ||
            targetingMode.TagsListContains(part.def.tags);


        public static BodyPartRecord RerollBodyPart(TargetingModeDef targetingMode, BodyPartRecord bodyPart, DamageInfo dinfo, Pawn pawn) =>
            RerollBodyPart(targetingMode, bodyPart, dinfo.Def, dinfo.Height, dinfo.Depth, pawn);

        public static BodyPartRecord RerollBodyPart(TargetingModeDef targetingMode, BodyPartRecord bodyPart, DamageDef damDef, BodyPartHeight height, BodyPartDepth depth, Pawn pawn)
        {
            for (int i = 0; i < targetingMode.RerollCount(pawn); i++)
            {
                BodyPartRecord newPart = pawn.health.hediffSet.GetRandomNotMissingPart(damDef, height, depth);
                if (newPart.IsPrioritizedPart(targetingMode))
                    return newPart;
            }
            return bodyPart;
        }

    }
}
