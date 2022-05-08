using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System;

namespace Celezt.DialogueSystem.Editor
{
     [ScriptedImporter(1, FILE_EXTENSION)]
    public class DGImporter : ScriptedImporter
    {
        public const string FILE_EXTENSION = ".dialoguegraph";

        private const string DIALOGUE_GRAPH_ICON_PATH = "Packages/com.celezt.asyncdialogue/Editor/Resources/Icons/dg_graph_icon.png";
        public override void OnImportAsset(AssetImportContext ctx)
        {
            ReadOnlySpan<char> content = File.ReadAllText(ctx.assetPath);

            Dialogue mainObject = ScriptableObject.CreateInstance<Dialogue>().Initialize(content);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(DIALOGUE_GRAPH_ICON_PATH);
            ctx.AddObjectToAsset("MainAsset", mainObject, texture);
            ctx.SetMainObject(mainObject);
        }
    }
}
