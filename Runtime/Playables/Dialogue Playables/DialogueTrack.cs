using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.2f, 0.4f, 0.5f)]
    [TrackClipType(typeof(DialogueAsset))]
    [TrackBindingType(typeof(DialogueSystemBinder))]
    public class DialogueTrack : DSTrackAsset
    {

    }
}
