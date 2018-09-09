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

        public Pawn Pawn => parent as Pawn;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (parent.Faction != Faction.OfPlayer ||
                (Pawn != null && ((!Pawn.RaceProps.Animal && !Pawn.Drafted) || (Pawn.RaceProps.Animal && Pawn.training.HasLearned(TrainableDefOf.Obedience)))))
                yield break;

            yield return TargetingModesUtility.SetTargetModeCommand(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _targetingMode, "targetingMode");
        }

        public TargetingModeDef GetTargetingMode() => _targetingMode;

        public void SetTargetingMode(TargetingModeDef targetMode) => _targetingMode = targetMode;

        private TargetingModeDef _targetingMode = TargetingModeDefOf.Standard;

    }
}
