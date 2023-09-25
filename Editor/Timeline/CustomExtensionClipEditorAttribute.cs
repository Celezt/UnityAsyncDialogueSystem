using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class CustomExtensionClipEditorAttribute : Attribute
    {
        public Type BindType { get; private set; }

        public CustomExtensionClipEditorAttribute(Type bindType) 
        {
            BindType = bindType;
        }
    }
}
