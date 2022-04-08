using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class SerializationUtility
    {
        public static ReadOnlySpan<char> Serialize(GraphSerializeData serializeData) => 
            JsonConvert.SerializeObject(serializeData, Formatting.Indented);
    }
}
