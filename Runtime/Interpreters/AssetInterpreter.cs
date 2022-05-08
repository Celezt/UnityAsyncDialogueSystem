using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    /// <summary>
    /// Interpret asset as a timeline. Handles how the timeline should be created.
    /// </summary>
    public abstract class AssetInterpreter : IDSAsset
    {
        public abstract void OnImport(DSNode node, TimelineAsset timeline);
    }
}
