using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class BasePortType : IPortType
    {
        public virtual Color Color => new Color(0.8f, 0.8f, 0.8f);
    }
}
