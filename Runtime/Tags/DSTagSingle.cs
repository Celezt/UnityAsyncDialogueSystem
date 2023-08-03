using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSTagSingle : TagSingle
    {
        public DialogueAsset Asset => (DialogueAsset)Bind;

        public virtual void OnCreate(int index, DialogueAsset asset) { }
        public virtual void OnInvoke(int index, DialogueAsset asset) { }

        public sealed override void Awake(int index, object bind)
        {
            base.Awake(index, bind);

            OnCreate(index, (DialogueAsset)bind);
        }

        internal void Internal_OnInvoke() => OnInvoke(Index, Asset);
    }
}
