using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public abstract class Extension : IExtension
    {
        public UnityEngine.Object Reference
        {
            get => _reference;
            set => _reference = value;
        }

        [SerializeField, HideInInspector]
        private UnityEngine.Object _reference;
    }
}
