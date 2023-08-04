using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public interface ITagSystem<T> where T : ITag
    {
        public void Execute(IReadOnlyList<T> entities);
    }
}
