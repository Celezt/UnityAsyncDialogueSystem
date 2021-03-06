using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BlackboardPropertyAttribute : Attribute
    {
        public string CustomTypeName { get; set; }

        public BlackboardPropertyAttribute(string customTypeName = null)
        {
            CustomTypeName = customTypeName;
        }
    }
}
