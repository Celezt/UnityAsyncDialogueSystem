using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ActionEventAsset : DSPlayableAsset<ActionEventBehaviour>
    {
        private void OnDestroy()
        {
            (BehaviourReference as ActionEventBehaviour).Reciver?.ActionBinderDictionary.Remove(this);
        }
    }
}
