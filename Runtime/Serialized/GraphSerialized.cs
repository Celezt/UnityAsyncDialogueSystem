using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public struct GraphSerialized
    {
        public int DGVersion;
        public string ObjectID;
        public List<dynamic> Properties;
        public List<NodeSerialized> Nodes;
        public List<EdgeSerialized> Edges;
        public List<SerializedVector2Int> Positions;
        public List<dynamic> Data;
    }
}
