using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{
    public class CompTargetingMode : ThingComp, ITargetModeSettable
    {

        public Pawn Pawn => (Pawn)parent;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Pawn.Faction != Faction.OfPlayer || !Pawn.Drafted)
                yield break;

            yield return TargetModeSettableUtility.SetTargetModeCommand(this);
        }

        public TargetingModeDef GetTargetingMode()
        {
            return targetingMode;
        }

        public void SetTargetingMode(TargetingModeDef targetMode)
        {
            targetingMode = targetMode;
        }

        private TargetingModeDef targetingMode = TargetingModeDefOf.Head;

    }
}
