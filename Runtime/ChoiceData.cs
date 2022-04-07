using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChoiceData
{
    [field: SerializeField] public string NodeID { get; set; }
    [field: SerializeField] public string Text { get; set; }
}
