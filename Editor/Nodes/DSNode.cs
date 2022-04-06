using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class DSNode : Node
    {
        public abstract void Initialize(Vector2 position);
        public abstract void Draw();
    }
}
