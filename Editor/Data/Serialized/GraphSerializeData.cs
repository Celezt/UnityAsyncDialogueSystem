using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;

namespace Celezt.DialogueSystem.Editor
{
    [Serializable]
    public struct GraphSerializeData
    {
        public int DGVersion;
        public string ObjectID;
        public List<dynamic> Properties;
        public List<NodeSerializeData> Nodes;
        public List<EdgeSerializeData> Edges;
        public List<SerializedVector2Int> Positions;
        public List<dynamic> CustomSaveData;
    }
}
