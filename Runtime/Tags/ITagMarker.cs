using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagMarker : ITag
    {
        public void OnInvoke(int intex, DSPlayableAsset playableAsset, string? parameter = null);
    }
}
