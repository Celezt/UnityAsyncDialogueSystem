using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSTagSpan : TagSpan
    {
        public DialogueAsset Asset => (DialogueAsset)Bind;

        public virtual void OnCreate(RangeInt range, DialogueAsset asset) { }
        public virtual void OnEnter(int index, RangeInt range, DialogueAsset asset) { }
        public virtual void OnProcess(int index, RangeInt range, DialogueAsset asset) { }
        public virtual void OnExit(int index, RangeInt range, DialogueAsset asset) { }

        public sealed override void Awake(RangeInt range, object bind)
        { 
            base.Awake(range, bind);

            OnCreate(range, (DialogueAsset)bind);
        }

        internal void Internal_OnEnter(int index) => OnEnter(index, Range, Asset);
        internal void Internal_OnProcess(int index) => OnProcess(index, Range, Asset);
        internal void Internal_OnExit(int index) => OnExit(index, Range, Asset);
    }
}
