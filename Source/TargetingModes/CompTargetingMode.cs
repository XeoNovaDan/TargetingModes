using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
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
            if (_targetingMode == null || CanResetTargetingMode())
                _targetingMode = TargetingModesUtility.DefaultTargetingMode;
        }

        public bool CanResetTargetingMode()
        {
            // Non-player world pawns are exempt to player restrictions
            if (parent.Faction != Faction.OfPlayer && parent.Map == null)
                return Find.TickManager.TicksGame % TargModeResetAttemptInterval(3) == 0;

            // If player's set it to never reset, if it isn't time to update or if it's already been reset
            if (TargetingModesSettings.TargModeResetFrequencyInt == 0 ||
                Find.TickManager.TicksGame % TargModeResetAttemptInterval(TargetingModesSettings.TargModeResetFrequencyInt) != 0 ||
                _targetingMode == TargetingModesUtility.DefaultTargetingMode)
                return false;

            // Not super efficient code, but legibility's the priority
            if (Pawn != null)
            {
                if (Pawn.drafter?.Drafted == false)
                    return true;
                if (!GenAI.InDangerousCombat(Pawn))
                    return true;
            }
            if (parent is Building_TurretGun turret)
            {
                if (turret.CurrentTarget == null)
                    return true;
            }

            return false;
        }

        private static int TargModeResetAttemptInterval(int freqInt)
        {
            switch (freqInt)
            {
                // 1 = 1d
                case 1:
                    return GenDate.TicksPerDay;
                // 2 = 12h
                case 2:
                    return GenDate.TicksPerHour * 12;
                // 3 = 6h
                case 3:
                    return GenDate.TicksPerHour * 6;
                // 4 = 3h
                case 4:
                    return GenDate.TicksPerHour * 3;
                // other (5) = 1h
                default:
                    return GenDate.TicksPerHour;
            };
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
