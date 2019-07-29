using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{
    public class BodyDefExtension : DefModExtension
    {

        public Dictionary<TargetingModeDef, float> targetModeRerollCountFactors;

    }
}
