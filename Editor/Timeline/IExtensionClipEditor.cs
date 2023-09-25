using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem.Editor
{
    public interface IExtensionClipEditor
    {
        public int Order { get; }

        public void OnDrawBackground(TimelineClip clip, ClipBackgroundRegion region, IExtension extension);
    }
}
