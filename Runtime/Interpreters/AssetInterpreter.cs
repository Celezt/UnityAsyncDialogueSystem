using System;
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
        public DSNode Node => _node;
        public DSNode PreviousNode => _previousNode;

        internal DSNode _node;

        private DSNode _previousNode;

        protected abstract void OnInterpret(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline);
        protected abstract void OnNext(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline);

        public void OnInterpret(DialogueSystem system, DSNode currentNode)
        {
            _previousNode = currentNode;
            OnInterpret(_node, currentNode, system.CurrentDialogue, system, (TimelineAsset)system.Director.playableAsset);
        }

        public void OnNext(DialogueSystem system)
        {
            OnNext(_node, _previousNode, system.CurrentDialogue, system, (TimelineAsset)system.Director.playableAsset);
        }

        public static implicit operator DSNode(AssetInterpreter interpreter) => interpreter.Node;
    }
}
