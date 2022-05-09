using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    /// <summary>
    /// Interpret asset as a timeline. Handles how the timeline should be created.
    /// </summary>
    public abstract class AssetInterpreter : IDSAsset
    {
        internal DSNode _node;

        protected abstract void OnInterpret(DSNode node, DialogueSystem system, PlayableDirector director, TimelineAsset timeline);

        public void OnInterpret(DialogueSystem system)
        {
            OnInterpret(_node, system, system.Director, (TimelineAsset)system.Director.playableAsset);
        }
    }
}
