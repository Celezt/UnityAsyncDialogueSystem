using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class DSNode : Node
    {
        public string ID => Guid.NewGuid().ToString();
        public abstract void Initialize(GraphView graphView, Vector2 position);
        public abstract void Draw();
    }
}
