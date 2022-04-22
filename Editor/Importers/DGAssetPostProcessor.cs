using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace Celezt.DialogueSystem.Editor
{
    public class DGAssetPostProcessor : AssetPostprocessor
    {
        private static void UpdateAfterAssetChange(string[] newNames)
        {
            DGEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DGEditorWindow>();
            foreach (DGEditorWindow window in windows)
            {
                for (int i = 0; i < newNames.Length; i++)
                {
                    if (window.SelectedGuid == AssetDatabase.GUIDFromAssetPath(newNames[i]))
                        window.UpdateTitle();
                }
            }
        }

        private static void DisplayDeletionDialogue(string[] deletedAssets)
        {
            DGEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DGEditorWindow>();
            foreach (DGEditorWindow window in windows)
            {
                for (int i = 0; i < deletedAssets.Length; i++)
                {
                    if (window.SelectedGuid == AssetDatabase.GUIDFromAssetPath(deletedAssets[i]))
                        window.AssetWasDeleted();
                }
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Moved assets
            bool anyMovedDialogues = movedAssets.Any(x => x.EndsWith(DGImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            anyMovedDialogues |= movedAssets.Any(x => x.EndsWith(DGImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            if (anyMovedDialogues)
                UpdateAfterAssetChange(movedAssets);

            // Deleted assets
            bool anyRemovedDialogues = deletedAssets.Any(x => x.EndsWith(DGImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            anyRemovedDialogues |= deletedAssets.Any(x => x.EndsWith(DGImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            if (anyRemovedDialogues)
                DisplayDeletionDialogue(deletedAssets);

            DGEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DGEditorWindow>();

            List<GUID> changedGraphGuids = importedAssets
                .Where(x => x.EndsWith(DGImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase))
                .Select(AssetDatabase.GUIDFromAssetPath)
                .ToList();

            foreach (var window in windows)
            {
                if (changedGraphGuids.Contains(window.SelectedGuid))
                    window.CheckForChanges();
            }
        }
    }
}
