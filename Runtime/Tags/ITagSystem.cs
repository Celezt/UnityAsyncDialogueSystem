using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagSystem<TTag, TBind> where TTag : ITag where TBind : new()
    {
        public void OnCreate(IReadOnlyList<TTag> entities, TBind? binder);
    }
}
