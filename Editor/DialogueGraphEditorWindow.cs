using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    using Utilities;

    public class DialogueGraphEditorWindow : EditorWindow
    {
        internal const int DG_VERSION = 1;

        [SerializeField] GUID _selectedGuid;
        [SerializeField] string _lastSerializedContent;

        internal GUID SelectedGuid
        {
            get => _selectedGuid;
            set => _selectedGuid = value;
        }

        internal bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            set => hasUnsavedChanges = value;
        }

        internal bool HasChangesSinceLastSerialization
        {
            get
            {
                ReadOnlySpan<char> currentSerializedJson = SerializationUtility.Serialize(DG_VERSION, SelectedGuid, _graphView.nodes, _graphView.edges);
                return !MemoryExtensions.Equals(currentSerializedJson, _lastSerializedContent, StringComparison.Ordinal);
            }
        }
        
        internal bool AssetFileExist => File.Exists(AssetDatabase.GUIDToAssetPath(SelectedGuid));

        /// <summary>
        /// If graph is about to be saved.
        /// </summary>
        internal event Action OnSaveChanges = delegate { };
        /// <summary>
        /// If graph is about to be saved as a new asset.
        /// </summary>
        internal event Action OnSaveAsChanges = delegate { };

        private string SaveChangeMessage => 
            "Do you want to save the changes you made in the Dialogue Graph?\n\n" +
            AssetDatabase.GUIDToAssetPath(SelectedGuid.ToString()) +
            "\n\nYour changes will be lost if you don't save them.";

        private DialogueGraphView _graphView;

        public DialogueGraphEditorWindow Initialize(GUID assetGuid)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(assetGuid));

            if (asset == null)
                return this;

            if (!EditorUtility.IsPersistent(asset))
                return this;

            if (SelectedGuid == assetGuid)
                return this;

            Texture2D icon = icon = Resources.Load<Texture2D>("Icons/dg_graph_icon_gray_dark");
            titleContent.image = icon;

            SelectedGuid = assetGuid;

            UpdateTitle();
            Repaint();
            
            return this;
        }

        public override void SaveChanges()
        {
            Debug.Log("SaveChanges");
            Save();
            base.SaveChanges();
        }

        public void UpdateTitle()
        {
            ReadOnlySpan<char> assetPath = AssetDatabase.GUIDToAssetPath(SelectedGuid);
            ReadOnlySpan<char> title = Path.GetFileNameWithoutExtension(assetPath);

            if (HasChangesSinceLastSerialization)
            {
                hasUnsavedChanges = true;
                saveChangesMessage = SaveChangeMessage;
            }
            else
            {
                hasUnsavedChanges = false;
                saveChangesMessage = "";
            }

            if (!AssetFileExist)
                title = title.ToString() + "(deleted)";

            titleContent.text = title.ToString();
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }

        private void AddGraphView()
        {
            _graphView = new DialogueGraphView(this);
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(() => Save())
            {
                text = "Save Asset",
            });
            toolbar.Add(new ToolbarSpacer());
            toolbar.Add(new ToolbarButton(() => SaveAs())
            {
                text = "Save As..."
            });

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheet("DSVariablesStyles");
        }

        private bool SaveOnQuit()
        {
            if (!AssetFileExist)
                DisplayDeleteDialogue(false);

            if (hasUnsavedChanges)
            {
                int option = EditorUtility.DisplayDialogComplex(
                    "Dialogue Graph has been modified",
                    SaveChangeMessage,
                    "Save", "Cancel", "Discard Changes");

                return option switch
                {
                    0 => Save(),
                    1 => false,
                    _ => true,
                };
            }

            return true;
        }

        private bool DisplayDeleteDialogue(bool reopen = true)
        {
            bool save = false;
            bool close = false;
            while (true)
            {
                int option = EditorUtility.DisplayDialogComplex(
                            "Graph removed from project",
                            "The file has been deleted or removed from the project folder\n\n" +
                            AssetDatabase.GUIDToAssetPath(SelectedGuid) +
                            "\n\nWould you like to save your Graph Asset?",
                            "Save As...", "Cancel", "Discard Graph and Close Window");
                if (option == 0)
                {
                    ReadOnlySpan<char> savePath = SaveAsImplementation();
                    if (!savePath.IsEmpty)
                    {
                        save = true;
                        SelectedGuid = reopen ? AssetDatabase.GUIDFromAssetPath(savePath.ToString()) : new GUID();
                        break;
                    }
                }
                else if (option == 1)
                {
                    break;
                }
                else if (option == 2)
                {
                    close = true;
                    SelectedGuid = new GUID();
                    break;
                }
            }

            return (save || close);
        }

        private bool Save()
        {
            if (SelectedGuid.Empty())
                return false;

            OnSaveChanges.Invoke();

            ReadOnlySpan<char> FilePath = AssetDatabase.GUIDToAssetPath(SelectedGuid);

            if (FilePath.IsEmpty || FilePath.IsWhiteSpace())
                return false;

            ReadOnlySpan<char> serializedJSON = SerializationUtility.Serialize(DG_VERSION, SelectedGuid, _graphView.nodes, _graphView.edges);
            DialogueGraphCreator.Overwrite(FilePath, serializedJSON);
            hasUnsavedChanges = false;

            return true;
        }

        private void SaveAs()
        {
            OnSaveAsChanges.Invoke();
            SaveAsImplementation();
        }

        private ReadOnlySpan<char> SaveAsImplementation()
        {
            ReadOnlySpan<char> saveFilePath = ReadOnlySpan<char>.Empty;

            if (!SelectedGuid.Empty())
            {
                ReadOnlySpan<char> oldFilePath = AssetDatabase.GUIDToAssetPath(SelectedGuid);

                if (oldFilePath.IsEmpty)
                    return ReadOnlySpan<char>.Empty;

                ReadOnlySpan<char> newFilePath = EditorUtility.SaveFilePanelInProject(
                    "Save Graph As...",
                    Path.GetFileNameWithoutExtension(oldFilePath).ToString(),
                    DialogueGraphCreator.FILE_EXTENSION.Substring(1),   // Remove dot.
                    "",
                    Path.GetDirectoryName(oldFilePath).ToString()
                ).Replace(Application.dataPath, "Assets");  // Simplify path.

                if (newFilePath.IsEmpty || newFilePath.IsWhiteSpace())
                    return ReadOnlySpan<char>.Empty;

                if (!MemoryExtensions.Equals(newFilePath, oldFilePath, StringComparison.Ordinal))
                {
                    ReadOnlySpan<char> serializedJSON = SerializationUtility.Serialize(DG_VERSION, SelectedGuid, _graphView.nodes, _graphView.edges);

                    if (DialogueGraphCreator.Create(newFilePath, serializedJSON))
                    {
                        AssetDatabase.ImportAsset(newFilePath.ToString());
                        DialogueGraphImporterEditor.ShowGraphEditorWindow(newFilePath.ToString());
                        saveFilePath = newFilePath;
                    }
                }
                else
                {
                    Save();

                    saveFilePath = oldFilePath;
                }
            }

            return saveFilePath;
        }
    }
}
