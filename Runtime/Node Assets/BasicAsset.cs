using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Basic Asset", menuName = "Dialogue/Assets/Basic Asset")]
    public class BasicAsset : NodeAsset
    {
        public object Value;

        public override object Process(object[] inputs, int outputIndex)
        {
            return Value;
        }
    }
}
