using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AssetBinderAttribute : Attribute
    {
        internal Type AssetType { get; private set; }

        public AssetBinderAttribute(Type assetType)
        {
            AssetType = assetType;
        }
    }
}
