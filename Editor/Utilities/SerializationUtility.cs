using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class SerializationUtility
    {
        public const string FILE_EXTENSION = ".dialoguegraph";

        public static ReadOnlySpan<char> Serialize(GraphSerializeData serializeData) => 
            JsonConvert.SerializeObject(serializeData, Formatting.Indented);

        public static void WriteToFile(ReadOnlySpan<char> serializedData)
        {

        }
    }
}
