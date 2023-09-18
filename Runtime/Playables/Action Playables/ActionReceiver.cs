using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    [DisallowMultipleComponent]
    public class ActionReceiver : MonoBehaviour
    {
        public SerializableDictionary<UnityEngine.Playables.PlayableAsset, ActionBinder> ActionBinderDictionary => _actionBinderDictionary;

        [SerializeField]
        private SerializableDictionary<UnityEngine.Playables.PlayableAsset, ActionBinder> _actionBinderDictionary = new SerializableDictionary<UnityEngine.Playables.PlayableAsset, ActionBinder>();

        [Serializable]
        public struct ActionBinder
        {
            public UnityEvent OnEnter;
            public UnityEvent OnExit;
        }
    }
}