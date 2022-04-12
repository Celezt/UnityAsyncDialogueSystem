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
        [JsonRequired] public int DGVersion;
        [JsonRequired] public string ObjectID;
        [JsonRequired] public List<NodeSerializeData> Nodes;
        [JsonRequired] public List<EdgeSerializeData> Edges;
        [JsonRequired] public List<SerializedVector2Int> Positions;
        [JsonRequired] public List<dynamic> CustomSaveData;
    }
}
