using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Celezt.DialogueSystem
{
    public static class DSUtility
    {
        public static GraphSerialized DeserializeGraph(ReadOnlySpan<char> serializedData)
        {
            return JsonConvert.DeserializeObject<GraphSerialized>(serializedData.ToString());
        }
    }
}
