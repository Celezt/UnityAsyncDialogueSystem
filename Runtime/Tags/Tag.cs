using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public abstract class Tag : ITag
    {
        public object? Bind { get; protected set; }
 
        public void AddPadding(float padding)
        {
            
        }
    }
}
