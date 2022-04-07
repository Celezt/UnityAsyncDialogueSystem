using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public struct NodeData
    {
        [field: SerializeField] public Guid ID { get; set; }
        [field: SerializeField] public string TypeFullName { get; set; }
        [field: SerializeField] public string ActorID { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<ChoiceData> Choices { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}
