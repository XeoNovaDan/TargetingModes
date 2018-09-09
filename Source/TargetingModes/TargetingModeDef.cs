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

        public bool HasNoSpecifiedPartDetails =>
            parts.NullOrEmpty() && partsOrAnyChildren.NullOrEmpty() && tags.NullOrEmpty();

        public bool PartsListContains(BodyPartDef def) => !parts.NullOrEmpty() && parts.Contains(def);

        public bool PartsOrAnyChildrenListContains(BodyPartRecord part)
        {
            if (!partsOrAnyChildren.NullOrEmpty())
            {
                if (partsOrAnyChildren.Contains(part.def))
                    return true;

                if (part.IsCorePart)
                    return partsOrAnyChildren.Contains(part.def);

                BodyPartRecord ancestor = part;
                while (ancestor.parent != null)
                {
                    ancestor = ancestor.parent;
                    if (partsOrAnyChildren.Contains(ancestor.def))
                        return true;
                }
            }
            return false;
        }

        public bool TagsListContains(List<BodyPartTagDef> partTags)
        {
            if (!tags.NullOrEmpty())
                foreach (BodyPartTagDef tag in partTags)
                {
                    if (tags.Contains(tag))
                        return true;
                }
            return false;
        }

        public int RerollCount(Pawn pawn)
        {
            var extensionValues = pawn.RaceProps.body.GetModExtension<TargetModeRerollFactors>();
            return GenMath.RoundRandom(rerollCount *
                ((extensionValues != null && extensionValues.targetModeRerollCountFactors.ContainsKey(this)) ?
                extensionValues.targetModeRerollCountFactors[this] : 1f));
        }

        public override void PostLoad()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                if (!iconPath.NullOrEmpty())
                    uiIcon = ContentFinder<Texture2D>.Get(iconPath);
            });
        }

        public float commonality = 1f;

        public string iconPath;

        public int displayOrder;

        public float hitChanceFactor = 1f;

        private int rerollCount = 0;

        public List<BodyPartDef> parts;

        public List<BodyPartDef> partsOrAnyChildren;

        public List<BodyPartTagDef> tags;

        [Unsaved]
        public Texture2D uiIcon = BaseContent.BadTex;

    }
}
