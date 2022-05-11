using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Celezt.DialogueSystem
{
    public class ButtonAsset : ActionAsset
    {
        [HideInInspector]
        public DialogueSystem System;
        public ExposedReference<ButtonBinder> ButtonReference;
        public string Text;
        public AssetProcessor Condition;
        public ActionPlayableSettings Settings;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new ButtonBehaviour();
        }

        private void OnDestroy()
        {
            if (BehaviourReference != null)
                ((ButtonBehaviour)BehaviourReference).Hide();
        }
    }
}
