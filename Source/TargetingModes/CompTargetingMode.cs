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
            if (parent.Faction == Faction.OfPlayer && (Pawn == null ||
                (Pawn.training?.HasLearned(TrainableDefOf.Obedience) == true ||
                Pawn.Drafted)))
                yield return TargetingModesUtility.SetTargetModeCommand(this);
        }

        public override void CompTick()
        {
            base.CompTick();
            // Debugging
            //if (Find.TickManager.TicksGame % (GenTicks.TicksPerRealSecond * 4) == 0)
            //    Log.Message(this.ToString());
            // For compatibility with existing saves
            if (_targetingMode == null)
                _targetingMode = TargetingModesUtility.DefaultTargetingMode;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _targetingMode, "targetingMode");
        }

        public override string ToString() =>
            $"CompTargetingMode for {parent} :: _targetingMode = {_targetingMode.LabelCap}";

        public TargetingModeDef GetTargetingMode() => _targetingMode;

        public void SetTargetingMode(TargetingModeDef targetMode) => _targetingMode = targetMode;

        private TargetingModeDef _targetingMode = TargetingModesUtility.DefaultTargetingMode;

    }
}
