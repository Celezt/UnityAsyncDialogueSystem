using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    using Utilities;

    public class DialogueGraphCreator
    {
        [MenuItem("Assets/Create/Dialogue Graph", priority = 90)]
        public static void CreateEmptySelected() => CreateNewSelected("{\n\n}");

        public static void CreateNewSelected(ReadOnlySpan<char> content)
        {
            string[] selectedGUIDs = Selection.assetGUIDs;

            if (selectedGUIDs.Length == 0) // Nothing is selected.
                return;

            string path = AssetDatabase.GUIDToAssetPath(selectedGUIDs[0]);

            if (File.Exists($"{path}/New Dialogue Graph{SerializationUtility.FILE_EXTENSION}"))
            {
                int index = 1;
                do
                {
                    string fullName = $"{path}/New Dialogue Graph {index}{SerializationUtility.FILE_EXTENSION}";
                    if (!File.Exists(fullName))
                    {
                        File.WriteAllText(fullName, content.ToString());
                        break;
                    }

                } while (++index < int.MaxValue);
            }
            else
                File.WriteAllText($"{path}/New Dialogue Graph{SerializationUtility.FILE_EXTENSION}", content.ToString());

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Create new or overwrite already existing file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns>If already exist.</returns>
        public static bool CreateOrOverwrite(ReadOnlySpan<char> path, ReadOnlySpan<char> content)
        {
            string fullPath;
            if (Path.HasExtension(path))
            {
                ReadOnlySpan<char> extension = Path.GetExtension(path);
                if (extension == SerializationUtility.FILE_EXTENSION)
                    fullPath = path.ToString();
                else
                    throw new ArgumentException($"\"{extension.ToString()}\" wrong extension");
            }
            else
                fullPath = path.ToString() + SerializationUtility.FILE_EXTENSION;

            bool exist = File.Exists(fullPath);
            File.WriteAllText(fullPath, content.ToString());

            AssetDatabase.Refresh();

            return exist;
        }
    }
}
