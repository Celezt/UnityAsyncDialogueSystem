using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [Serializable]
    public struct GraphSerializeData
    {
        public NodeSerializeData[] Nodes;
        public EdgeSerializeData[] Edges;
    }
}
