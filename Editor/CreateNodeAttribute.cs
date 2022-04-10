using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CreateNodeAttribute : Attribute
    {
        internal string MenuName { get; private set; }

        public CreateNodeAttribute(string menuName)
        {
            MenuName = menuName;
        }
    }
}
