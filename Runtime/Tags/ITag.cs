using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITag
    {
        public object? Binder { get; }

        public void OnCreate();
    }
}
