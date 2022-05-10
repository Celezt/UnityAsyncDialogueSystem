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

        protected abstract void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline);
        protected abstract void OnNext(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline);

        public void OnInterpret(DialogueSystem system)
        {
            system._previousNodes.Add(_node);
            OnInterpret(_node, system._previousNodes, system.CurrentDialogue, system, (TimelineAsset)system.Director.playableAsset);
        }

        public void OnNext(DialogueSystem system)
        {
            system._previousNodes.Add(_node);
            OnNext(_node, system._previousNodes, system.CurrentDialogue, system, (TimelineAsset)system.Director.playableAsset);
        }
    }
}
