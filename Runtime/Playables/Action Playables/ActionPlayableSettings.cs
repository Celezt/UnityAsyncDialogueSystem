using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Action Playable Settings", menuName = "Dialogue/Action Playable Settings")]
    public class ActionPlayableSettings : ScriptableObject
    {
        public AnimationCurve StartFade = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve EndFade = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public float StartTimeOffset => StartFade.length > 0 ? StartFade.keys.Last().time : 0;
        public float EndTimeOffset => EndFade.length > 0 ? EndFade.keys.Last().time : 0;
    }
}
