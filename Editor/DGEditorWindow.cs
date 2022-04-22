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

    public class DGEditorWindow : EditorWindow
    {
        internal const int DG_VERSION = 1;

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
                ReadOnlySpan<char> currentSerializedJson = JsonUtility.SerializeGraph(DG_VERSION, SelectedGuid, _graphView.nodes, _graphView.edges);
                return !MemoryExtensions.Equals(currentSerializedJson, _lastSerializedContent, StringComparison.Ordinal);
            }
        }

        internal bool HasAssetFileChanged => !ReadAssetFile().Equals(_lastSerializedContent, StringComparison.Ordinal);

        internal bool AssetFileExist => File.Exists(AssetDatabase.GUIDToAssetPath(SelectedGuid));

        private string SaveChangeMessage =>
            "Do you want to save the changes you made in the Dialogue Graph?\n\n" +
            AssetDatabase.GUIDToAssetPath(SelectedGuid.ToString()) +
            "\n\nYour changes will be lost if you don't save them.";

        [SerializeField] private GUID _selectedGuid;
        [SerializeField] private string _lastSerializedContent;
        [SerializeField] private bool _checkAssetStatus;

        [NonSerialized] private bool _isProTheme;

        /// <summary>
        /// If graph is about to be saved.
        /// </summary>
        internal event Action OnSaveChanges = delegate { };
        /// <summary>
        /// If graph is about to be saved as a new asset.
        /// </summary>
        internal event Action OnSaveAsChanges = delegate { };
        /// <summary>
        /// If graph name is has changed.
        /// </summary>
        internal event Action<string> OnTitleChanged = delegate { };

        private DGView _graphView;

        public DGEditorWindow Initialize(GUID assetGuid)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(assetGuid));

            if (asset == null)
                return this;

            if (!EditorUtility.IsPersistent(asset))
                return this;

            if (SelectedGuid == assetGuid)
                return this;

            string theme = EditorGUIUtility.isProSkin ? "_dark" : "_light";
            Texture2D icon = Resources.Load<Texture2D>("Icons/dg_graph_icon_gray" + theme);
            titleContent.image = icon;

            SelectedGuid = assetGuid;

            // Add graph view.
            {
                _graphView = new DGView(this);
                _graphView.StretchToParentSize();
                rootVisualElement.Add(_graphView);
            }

            // Add tool bar.
            {
                Toolbar toolbar = new Toolbar();
                toolbar.Add(new ToolbarButton(() => SaveAsset())
                {
                    text = "Save Asset",
                });
                toolbar.Add(new ToolbarSpacer());
                toolbar.Add(new ToolbarButton(() => SaveAssetAs())
                {
                    text = "Save As..."
                });

                _graphView.Add(toolbar);
            }

            // Add style sheet.
            _graphView.AddStyleSheet(StyleUtility.STYLE_PATH + "DGVariables");

            _lastSerializedContent = ReadAssetFile().ToString();
            _graphView.DeserializeGraph(_lastSerializedContent);       

            Repaint();
            
            return this;
        }

        public override void SaveChanges()
        {
            SaveAsset();
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
            OnTitleChanged.Invoke(title.ToString());
        }

        public void AssetWasDeleted()
        {
            _checkAssetStatus = true;
            UpdateTitle();
        }

        public void CheckForChanges()
        {
            if (!_checkAssetStatus)
            {
                _checkAssetStatus = true;
                UpdateTitle();
            }
        }

        private void Update()
        {
            bool updateTitle = false;

            if (_checkAssetStatus)                          // Check if any change has been made to the asset.
            {
                _checkAssetStatus = false;
                if (!AssetFileExist)
                    DisplayDeleteDialogue();
                else if (HasAssetFileChanged)
                {
                    bool graphChanged = HasChangesSinceLastSerialization;

                    if (EditorUtility.DisplayDialog(
                        "Graph has changed in file",
                        AssetDatabase.GUIDToAssetPath(SelectedGuid) + "\n\n" +
                        (graphChanged ? "Do you want to reload it and lose the changes made in the graph?" : "Do you want to reload it?"),
                        graphChanged ? "Discard Changes and Reload" : "Reload",
                        "Don't Reload"))
                    {
                        rootVisualElement.Remove(_graphView);
                        _graphView = null;
                    }
                }

                updateTitle = true;
            }

            if (EditorGUIUtility.isProSkin != _isProTheme)  // Swap icon if using pro theme.
            {
                updateTitle = true;
                _isProTheme = EditorGUIUtility.isProSkin;
            }

            if (_graphView == null && !SelectedGuid.Empty())    // Trigger reload if no graph exist but still selected.
            {
                GUID guid = SelectedGuid;
                SelectedGuid = new GUID();
                Initialize(guid);
            }

            if (_graphView == null)     // Close window if no graphView is present.
            {
                Close();
                return;
            }

            if (updateTitle)
                UpdateTitle();
        }   

        private void OnEnable()
        {
            this.SetAntiAliasing(4);
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
                    0 => SaveAsset(),
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
                    ReadOnlySpan<char> savePath = SaveAssetAsImplementation();
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

        private ReadOnlySpan<char> ReadAssetFile()
        {
           ReadOnlySpan<char> filePath = AssetDatabase.GUIDToAssetPath(_selectedGuid);
            return DialogueGraphCreator.ReadAll(filePath);
        }

        private bool SaveAsset()
        {
            if (SelectedGuid.Empty())
                return false;

            OnSaveChanges.Invoke();

            ReadOnlySpan<char> FilePath = AssetDatabase.GUIDToAssetPath(SelectedGuid);

            if (FilePath.IsEmpty || FilePath.IsWhiteSpace())
                return false;

            ReadOnlySpan<char> serializedJSON = JsonUtility.SerializeGraph(DG_VERSION, SelectedGuid, _graphView.nodes, _graphView.edges);
            DialogueGraphCreator.Overwrite(FilePath, serializedJSON);
            hasUnsavedChanges = false;

            return true;
        }

        private void SaveAssetAs()
        {
            OnSaveAsChanges.Invoke();
            SaveAssetAsImplementation();
        }

        private ReadOnlySpan<char> SaveAssetAsImplementation()
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
                    DGImporter.FILE_EXTENSION.Substring(1),   // Remove dot.
                    "",
                    Path.GetDirectoryName(oldFilePath).ToString()
                ).Replace(Application.dataPath, "Assets");  // Simplify path.

                if (newFilePath.IsEmpty || newFilePath.IsWhiteSpace())
                    return ReadOnlySpan<char>.Empty;

                if (!MemoryExtensions.Equals(newFilePath, oldFilePath, StringComparison.Ordinal))
                {
                    ReadOnlySpan<char> serializedJSON = JsonUtility.SerializeGraph(DG_VERSION, SelectedGuid, _graphView.nodes, _graphView.edges);

                    if (DialogueGraphCreator.Create(newFilePath, serializedJSON))
                    {
                        AssetDatabase.ImportAsset(newFilePath.ToString());
                        DGImporterEditor.ShowGraphEditorWindow(newFilePath.ToString());
                        saveFilePath = newFilePath;
                    }
                }
                else
                {
                    SaveAsset();

                    saveFilePath = oldFilePath;
                }
            }

            return saveFilePath;
        }
    }
}
