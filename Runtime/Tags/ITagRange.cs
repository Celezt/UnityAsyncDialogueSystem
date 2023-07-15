using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagRange : ITag
    {
        public void OnEnter(int intex, RangeInt range, DSPlayableAsset playableAsset, string? parameter = null);
        public void OnProcess(int intex, RangeInt range, DSPlayableAsset playableAsset, string? parameter = null);
        public void OnExit(int intex, RangeInt range, DSPlayableAsset playableAsset, string? parameter = null);
    }
}
