using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{
    public interface ITargetModeSettable
    {

        TargetingModeDef GetTargetingMode();

        void SetTargetingMode(TargetingModeDef targetMode);

    }
}
