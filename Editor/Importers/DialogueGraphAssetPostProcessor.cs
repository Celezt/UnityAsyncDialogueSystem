using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueGraphAssetPostProcessor : AssetPostprocessor
    {
        private static void UpdateAfterAssetChange(string[] newNames)
        {
            DialogueGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DialogueGraphEditorWindow>();
            foreach (DialogueGraphEditorWindow window in windows)
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
            DialogueGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DialogueGraphEditorWindow>();
            foreach (DialogueGraphEditorWindow window in windows)
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
            bool anyMovedDialogues = movedAssets.Any(x => x.EndsWith(DialogueGraphImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            anyMovedDialogues |= movedAssets.Any(x => x.EndsWith(DialogueGraphImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            if (anyMovedDialogues)
                UpdateAfterAssetChange(movedAssets);

            // Deleted assets
            bool anyRemovedDialogues = deletedAssets.Any(x => x.EndsWith(DialogueGraphImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            anyRemovedDialogues |= deletedAssets.Any(x => x.EndsWith(DialogueGraphImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            if (anyRemovedDialogues)
                DisplayDeletionDialogue(deletedAssets);

            DialogueGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DialogueGraphEditorWindow>();

            List<GUID> changedGraphGuids = importedAssets
                .Where(x => x.EndsWith(DialogueGraphImporter.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase))
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
