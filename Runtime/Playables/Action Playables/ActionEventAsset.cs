using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ActionEventAsset : ActionAsset
    {
        public ActionReceiver Receiver
        {
            get
            {
                if (Director == null)    // Director does not exist.
                    return null;

                if (_receiver == null) 
                    _receiver = Director.GetGenericBinding(Clip.GetParentTrack()) as ActionReceiver;

                return _receiver;
            }
        }

        public UnityEvent OnEnter
        {
            get
            {
                if (_receiver.ActionBinderDictionary.TryGetValue(this, out var value))
                    return value.OnEnter;

                return null;
            }
        }

        public UnityEvent OnExit
        {
            get
            {
                if (_receiver.ActionBinderDictionary.TryGetValue(this, out var value))
                    return value.OnExit;

                return null;
            }
        }

        private ActionReceiver _receiver;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {   
            return new ActionEventBehaviour();
        }

        private void OnDestroy()
        {
            if (BehaviourReference != null)
                Receiver?.ActionBinderDictionary.Remove(this);
        }
    }
}
