using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Celezt.DialogueSystem
{
    public class ButtonAsset : ActionAsset
    {
        public ExposedReference<Button> ButtonReference;
        public string Text;
        public AssetProcessor Condition;
        public ActionPlayableSettings Settings;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new ButtonBehaviour();
        }
    }
}
