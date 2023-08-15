using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CreateTagAttribute : Attribute
    {
        public int Order { get; private set; }

        public CreateTagAttribute(int order = 0)
        {
            Order = order;
        }
    }
}
