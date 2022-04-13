using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Celezt.DialogueSystem.Editor
{
    using Utilities;

     [ScriptedImporter(1, FILE_EXTENSION)]
    public class DialogueGraphImporter : ScriptedImporter
    {
        public const string FILE_EXTENSION = ".dialoguegraph";

        private const string DIALOGUE_GRAPH_ICON_PATH = "Packages/com.celezt.asyncdialogue/Editor/Resources/Icons/dg_graph_icon.png";
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Dialogue mainObject = ScriptableObject.CreateInstance<Dialogue>();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(DIALOGUE_GRAPH_ICON_PATH);
            ctx.AddObjectToAsset("MainAsset", mainObject, texture);
            ctx.SetMainObject(mainObject);
        }
    }
}
