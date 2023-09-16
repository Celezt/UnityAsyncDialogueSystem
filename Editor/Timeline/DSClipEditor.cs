using Celezt.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem.Editor
{
    [CustomTimelineEditor(typeof(DialogueAsset))]
    public class DSClipEditor : ClipEditor
    {
        private static Dictionary<Type, ExtensionDrawer> _extensionDrawers = new();
       
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as DialogueAsset;

        }
    }
}
