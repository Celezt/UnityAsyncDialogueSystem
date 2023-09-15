using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public interface IExtension
    {
        public UnityEngine.Object Target { get; set; }
        public UnityEngine.Object Reference { get; set; }
        public IReadOnlyDictionary<string, bool> PropertiesModified { get; }
    }
}
