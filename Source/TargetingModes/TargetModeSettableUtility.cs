using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{
    public static class TargetModeSettableUtility
    {

        public static Command_SetTargetingMode SetTargetModeCommand(ITargetModeSettable settable)
        {
            return new Command_SetTargetingMode
            {
                defaultDesc = "CommandSelectPlantToGrowDesc".Translate(),
                settable = settable
            };
        }

    }
}
