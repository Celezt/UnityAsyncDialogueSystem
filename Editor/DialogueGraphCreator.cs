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
        private const string EMPTY_FILE = "{\n\n}";

        [MenuItem("Assets/Create/Dialogue Graph", priority = 90)]
        static void Create()
        {
            string[] selectedGUIDs = Selection.assetGUIDs;

            if (selectedGUIDs.Length == 0) // Nothing is selected.
                return;

            string path = AssetDatabase.GUIDToAssetPath(selectedGUIDs[0]);

            if (File.Exists($"{path}/New Dialogue Graph{SerializationUtility.FILE_TYPE}"))
            {
                int index = 1;
                do
                {
                    string fullName = $"{path}/New Dialogue Graph {index}{SerializationUtility.FILE_TYPE}";
                    if (!File.Exists(fullName))
                    {
                        File.WriteAllText(fullName, EMPTY_FILE);
                        break;
                    }

                } while (++index < int.MaxValue);
            }
            else
                File.WriteAllText($"{path}/New Dialogue Graph{SerializationUtility.FILE_TYPE}", EMPTY_FILE);

            AssetDatabase.Refresh();
        }
    }
}
