using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.UIElements;
    using Utilities;

    public class DialogueGraphCreator
    {
        public const string FILE_EXTENSION = ".dialoguegraph";

        [MenuItem("Assets/Create/Dialogue Graph", priority = 90)]
        public static void CreateEmptySelected()
        {
            string[] selectedGUIDs = Selection.assetGUIDs;

            if (selectedGUIDs.Length == 0) // Nothing is selected.
                return;

            if (GUID.TryParse(selectedGUIDs[0], out GUID guid))
                CreateNewSelected(SerializationUtility.Serialize(DialogueGraphView.DG_VERSION, guid));
        }

        public static void CreateNewSelected(ReadOnlySpan<char> content)
        {
            string[] selectedGUIDs = Selection.assetGUIDs;

            if (selectedGUIDs.Length == 0) // Nothing is selected.
                return;

            string path = AssetDatabase.GUIDToAssetPath(selectedGUIDs[0]);

            if (File.Exists($"{path}/New Dialogue Graph{FILE_EXTENSION}"))
            {
                int index = 1;
                do
                {
                    string fullName = $"{path}/New Dialogue Graph {index}{FILE_EXTENSION}";
                    if (!File.Exists(fullName))
                    {
                        File.WriteAllText(fullName, content.ToString());
                        break;
                    }

                } while (++index < int.MaxValue);
            }
            else
                File.WriteAllText($"{path}/New Dialogue Graph{FILE_EXTENSION}", content.ToString());

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Create new file.
        /// </summary>
        /// <returns>If non-existent</returns>
        public static bool Create(ReadOnlySpan<char> path, ReadOnlySpan<char> content)
        {
            if (!TryGetValidPath(ref path))
                return false;

            if (File.Exists(path.ToString()))
                return false;

            return WriteToDisk(path, content);
        }

        /// <summary>
        /// Overwrite already existing file.
        /// </summary>
        /// <returns>If it already exist.</returns>
        public static bool Overwrite(ReadOnlySpan<char> path, ReadOnlySpan<char> content)
        {
            if (!TryGetValidPath(ref path))
                return false;
             
            if (!File.Exists(path.ToString()))
                return false;

            return WriteToDisk(path, content);
        }

        private static bool TryGetValidPath(ref ReadOnlySpan<char> path)
        {
            if (Path.HasExtension(path))
            {
                ReadOnlySpan<char> extension = Path.GetExtension(path);
                if (!MemoryExtensions.Equals(extension, FILE_EXTENSION, StringComparison.Ordinal))
                    return false;
            }
            else 
                path = path.ToString() + FILE_EXTENSION;

            return true;
        }

        private static bool WriteToDisk(ReadOnlySpan<char> path, ReadOnlySpan<char> content)
        {
            while (true)
            {
                try
                {
                    File.WriteAllText(path.ToString(), content.ToString());
                }
                catch (Exception e)
                {
                    if (e.GetBaseException() is UnauthorizedAccessException &&
                        (File.GetAttributes(path.ToString()) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        if (EditorUtility.DisplayDialog("File is Read-Only", path.ToString(), "Make Writable", "Cancel Save"))
                        {
                            // Make file writable.
                            FileInfo fileInfo = new FileInfo(path.ToString());
                            fileInfo.IsReadOnly = true;
                            continue;   // Retry.
                        }
                        else
                            return false;   // Cancel save.
                    }

                    Debug.LogException(e);

                    if (EditorUtility.DisplayDialog("Exception While Saving", e.ToString(), "Retry", "Cancel"))
                        continue;   // Retry;
                    else
                        return false; // Cancel save.

                }
                break;
            }

            return true;
        }
    }
}
