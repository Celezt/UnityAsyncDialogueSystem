using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(BasicAsset), true), CanEditMultipleObjects]
    public class BasicAssetEditor : NodeAssetEditor
    {
        public override void BuildInspector()
        {
            var asset = target as BasicAsset;

            switch (asset.Value)
            {
                case float floatValue:
                    float newValue = EditorGUILayout.FloatField("Value", floatValue);
                    if (newValue != floatValue)
                    {
                        asset.Value = newValue;
                        EditorUtility.SetDirty(asset);
                    }

                    break;
            }
        }
    }
}
