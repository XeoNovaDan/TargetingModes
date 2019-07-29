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
            // For compatibility with existing saves
            if (_targetingMode == null || CanResetTargetingMode())
                SetTargetingMode(TargetingModesUtility.DefaultTargetingMode);
        }

        public bool CanResetTargetingMode()
        {
            // Non-player world pawns are exempt to player restrictions
            if (parent.Map == null)
                return Find.TickManager.TicksGame % TargModeResetAttemptInterval() == 0;

            // If player's set it to never reset, if it isn't time to update or if it's already been reset
            if (TargetingModesSettings.TargModeResetFrequencyInt == 0 ||
                Find.TickManager.TicksGame != _resetTargetingModeTick ||
                _targetingMode == TargetingModesUtility.DefaultTargetingMode)
                return false;

            _resetTargetingModeTick += TargModeResetAttemptInterval();

            // Not super efficient code, but legibility's the priority
            if (Pawn != null)
            {
                if (Pawn.Drafted)
                    return false;
                if (GenAI.InDangerousCombat(Pawn))
                    return false;
            }
            if (parent is Building_TurretGun turret)
            {
                if (turret.CurrentTarget != null)
                    return false;
            }

            return true;
        }

        private int TargModeResetAttemptInterval()
        {
            switch ((parent.Faction == Faction.OfPlayer) ? TargetingModesSettings.TargModeResetFrequencyInt : 3)
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
                // other = 1h
                default:
                    return GenDate.TicksPerHour;
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _targetingMode, "targetingMode");
            Scribe_Values.Look(ref _resetTargetingModeTick, "resetTargetingModeTick", -1);
        }

        public override string ToString() =>
            $"CompTargetingMode for {parent} :: _targetingMode={_targetingMode.LabelCap}; _resetTargetingModeTick={_resetTargetingModeTick} (TicksGame={Find.TickManager.TicksGame})";

        public TargetingModeDef GetTargetingMode() => _targetingMode;

        public void SetTargetingMode(TargetingModeDef targetMode)
        {
            // Actually set the targeting mode
            _targetingMode = targetMode;

            // Set which tick the game will try to reset the mode at
            _resetTargetingModeTick = Find.TickManager.TicksGame + TargModeResetAttemptInterval();
        }

        private TargetingModeDef _targetingMode = TargetingModesUtility.DefaultTargetingMode;
        private int _resetTargetingModeTick = -1;

    }
}
