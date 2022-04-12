using System;
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
        internal const string GRAPH_NAME = "Dialogue Graph";

        internal GUID SelectedGuid { get; set; }
        internal bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            set => hasUnsavedChanges = value;
        }

        internal event Action OnSaveChanges = delegate { };

        private string SaveChangeMessage => 
            "Do you want to save the changes you made in the Dialogue Graph?\n\n" +
            AssetDatabase.GUIDToAssetPath(SelectedGuid.ToString()) +
            "\n\nYour changes will be lost if you don't save them.";

        public DialogueGraphEditorWindow Initialize(GUID assetGuid)
        {
            if (SelectedGuid == assetGuid)
                return this;

            SelectedGuid = assetGuid;
            titleContent.text = GRAPH_NAME;
            saveChangesMessage = SaveChangeMessage;
            
            return this;
        }

        public override void SaveChanges()
        {
            Save();
            base.SaveChanges();
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }

        private void AddGraphView()
        {
            DialogueGraphView graphView = new DialogueGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
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

        private bool Save()
        {
            HasUnsavedChanges = true;
            if (hasUnsavedChanges)
            {
                OnSaveChanges.Invoke();
                hasUnsavedChanges = false;
                return true;
            }

            return false;
        }

        private bool SaveAs()
        {
            if (hasUnsavedChanges)
            {
                OnSaveChanges.Invoke();
                hasUnsavedChanges = false;
                return true;
            }

            return false;
        }
    }
}
