using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{

    [StaticConstructorOnStartup]
    public class Command_SetTargetingMode : Command
    {

        private static Texture2D SetTargetingModeTex = ContentFinder<Texture2D>.Get("UI/TargetingModes/MultipleModes");
        public ITargetModeSettable settable;
        public List<ITargetModeSettable> settables;

        public Command_SetTargetingMode()
        {
            TargetingModeDef targetingMode = null;
            bool multiplePawnsSelected = false;
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                if (obj is ITargetModeSettable targetModeSettable)
                {
                    if (targetingMode != targetModeSettable.GetTargetingMode())
                    {
                        multiplePawnsSelected = true;
                        break;
                    }
                    targetingMode = targetModeSettable.GetTargetingMode();
                }
            }
            if (multiplePawnsSelected)
            {
                icon = SetTargetingModeTex;
                defaultLabel = "CommandSetTargetingModeMulti".Translate();
            }
            else
            {
                icon = targetingMode.uiIcon;
                defaultLabel = "CommandSetTargetingMode".Translate(targetingMode.LabelCap);
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            TargetingModes.SortBy((TargetingModeDef t) => t.displayOrder, (TargetingModeDef t) => t.label);
            List<FloatMenuOption> targetingModeOptions = new List<FloatMenuOption>();
            for (int i = 0; i < TargetingModes.Count; i++)
            {
                TargetingModeDef targetMode = TargetingModes[i];
                targetingModeOptions.Add(new FloatMenuOption(targetMode.LabelCap, null));
            }
            Find.WindowStack.Add(new FloatMenu(targetingModeOptions));
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (settables == null)
                settables = new List<ITargetModeSettable>();
            settables.Add(((Command_SetTargetingMode)other).settable);
            return false;
        }

        public List<TargetingModeDef> TargetingModes
        {
            get
            {
                List<TargetingModeDef> targetingModes = new List<TargetingModeDef>();
                foreach (TargetingModeDef targetMode in DefDatabase<TargetingModeDef>.AllDefsListForReading)
                    targetingModes.Add(targetMode);
                return targetingModes;
            }
        }

    }

}
