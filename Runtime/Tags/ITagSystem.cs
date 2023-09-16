using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagSystem<TTag, TBinder> where TTag : ITag
    {
        public void OnCreate(IReadOnlyList<TTag> entities, TBinder? binder);
    }
}
