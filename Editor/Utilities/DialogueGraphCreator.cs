using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public static class DialogueGraphCreator
    {
        [MenuItem("Assets/Create/Dialogue/Dialogue Graph", priority = 90)]
        public static void CreateEmptySelected()
        {
            string[] selectedGUIDs = Selection.assetGUIDs;

            if (selectedGUIDs.Length == 0) // Nothing is selected.
                return;

            if (GUID.TryParse(selectedGUIDs[0], out GUID guid))
                CreateNewSelected(JsonUtility.SerializeGraph(DGView.DG_VERSION, guid));
        }

        public static void CreateNewSelected(string content)
        {
            string[] selectedGUIDs = Selection.assetGUIDs;

            if (selectedGUIDs.Length == 0) // Nothing is selected.
                return;

            string path = AssetDatabase.GUIDToAssetPath(selectedGUIDs[0]);

            if (File.Exists($"{path}/New Dialogue Graph{DGImporter.FILE_EXTENSION}"))
            {
                int index = 1;
                do
                {
                    string fullName = $"{path}/New Dialogue Graph {index}{DGImporter.FILE_EXTENSION}";
                    if (!File.Exists(fullName))
                    {
                        File.WriteAllText(fullName, content);
                        break;
                    }

                } while (++index < int.MaxValue);
            }
            else
                File.WriteAllText($"{path}/New Dialogue Graph{DGImporter.FILE_EXTENSION}", content);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Create new file.
        /// </summary>
        /// <returns>If non-existent</returns>
        public static bool Create(string path, string content)
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
        public static bool Overwrite(string path, string content)
        {
            if (!TryGetValidPath(ref path))
                return false;
             
            if (!File.Exists(path))
                return false;

            return WriteToDisk(path, content);
        }

        public static string ReadAll(string path)
        {
            string  result = null;

            if (!IsValidPath(path))
                return result;

            try
            {
                result = File.ReadAllText(path);
            }
            catch
            {
                result = null;
            }

            return result;
        }

        private static bool IsValidPath(ReadOnlySpan<char> path)
        {
            if (Path.HasExtension(path))
            {
                ReadOnlySpan<char> extension = Path.GetExtension(path);
                if (MemoryExtensions.Equals(extension, DGImporter.FILE_EXTENSION, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool TryGetValidPath(ref string path)
        {
            if (Path.HasExtension(path))
            {
                ReadOnlySpan<char> extension = Path.GetExtension(path.AsSpan());
                if (!MemoryExtensions.Equals(extension, DGImporter.FILE_EXTENSION, StringComparison.Ordinal))
                    return false;
            }
            else 
                path = path + DGImporter.FILE_EXTENSION;

            return true;
        }

        private static bool WriteToDisk(string path, string content)
        {
            // Checks if asset is valid.
            if (Provider.enabled && Provider.isActive)
            {
                Asset asset = Provider.GetAssetByPath(path);
                if (asset != null)
                {
                    if (!Provider.IsOpenForEdit(asset))
                    {
                        Task task = Provider.Checkout(asset, CheckoutMode.Asset);
                        task.Wait();

                        if (!task.success)
                            Debug.Log(task.text + " " + task.resultCode);
                    }
                }
            }

            while (true)
            {
                try
                {
                    File.WriteAllText(path, content);
                }
                catch (Exception e)
                {
                    if (e.GetBaseException() is UnauthorizedAccessException &&
                        (File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        if (EditorUtility.DisplayDialog("File is Read-Only", path, "Make Writable", "Cancel Save"))
                        {
                            // Make file writable.
                            FileInfo fileInfo = new FileInfo(path);
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
