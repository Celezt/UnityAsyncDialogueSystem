using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor.Callbacks;

namespace Celezt.DialogueSystem.Editor
{
    using Utilities;

    [CustomEditor(typeof(DialogueGraphImporter))]
    public class DialogueGraphImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Dialogue Editor"))
            {
                AssetImporter importer = target as AssetImporter;
                Debug.Assert(importer != null, "importer != null");
                ShowGraphEditorWindow(importer.assetPath);
            }

            ApplyRevertGUI();
        }

        internal static bool ShowGraphEditorWindow(string path)
        {
            GUID guid = AssetDatabase.GUIDFromAssetPath(path);
            ReadOnlySpan<char> extension = Path.GetExtension(path);

            if (extension.IsEmpty || extension.IsWhiteSpace())
                return false;

            if (!MemoryExtensions.Equals(extension, DialogueGraphCreator.FILE_EXTENSION.AsSpan(), StringComparison.Ordinal))
                return false;

            foreach (var w in Resources.FindObjectsOfTypeAll<DialogueGraphEditorWindow>())
            {
                if (w.SelectedGuid == guid)
                {
                    w.Focus();
                    return true;
                }
            }

            var window = EditorWindow.CreateWindow<DialogueGraphEditorWindow>(typeof(DialogueGraphEditorWindow), typeof(SceneView));
            window.Initialize(guid);
            window.Focus();
            return true;
        }

        // Open asset when double clicking an asset in the Project Browser.
        [OnOpenAsset(0)]
        internal static bool OnOpenAsset(int instanceID, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);
            return ShowGraphEditorWindow(path);
        }
    }
}