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
        [HideInInspector]
        public string OverrideSettingName;
        public ExposedReference<ButtonBinder> ButtonReference;
        public string Text;
        public AssetProcessor Condition;
        public ActionPlayableSettings Settings;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new ButtonBehaviour();
        }

        protected override void OnDestroyClip()
        {
            if (BehaviourReference != null)
                ((ButtonBehaviour)BehaviourReference).Hide();
        }
    }
}
