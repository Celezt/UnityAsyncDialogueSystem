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
        public SerializableDictionary<PlayableAsset, ActionBinder> ActionBinderDictionary => _actionBinderDictionary;

        [SerializeField, HideInInspector]
        private SerializableDictionary<PlayableAsset, ActionBinder> _actionBinderDictionary = new SerializableDictionary<PlayableAsset, ActionBinder>();

        [Serializable]
        public struct ActionBinder
        {
            public UnityEvent OnEnter;
            public UnityEvent OnExit;
        }
    }
}