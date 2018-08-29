using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{
    public class TargetingModeDef : Def
    {

        public override void PostLoad()
        {
            if (!iconPath.NullOrEmpty())
                uiIcon = ContentFinder<Texture2D>.Get(iconPath);
        }

        public string iconPath;

        public int displayOrder;

        public List<BodyPartTagDef> tags;

        public List<BodyPartDef> parts;

        [Unsaved]
        public Texture2D uiIcon;

    }
}
