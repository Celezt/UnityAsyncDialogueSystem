using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class AwaitTag : ITagMarker
    {
        public void OnInvoke(int intex, DSPlayableAsset playableAsset, string parameter = null)
        {
            
        }
    }
}
