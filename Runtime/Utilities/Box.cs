using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public class Box<T> where T : new()
    {
        public T Wrapped
        {
            get => _wrapped;
            set => _wrapped = value;
        }

        [SerializeField]
        private T _wrapped = new T();
    }
}
