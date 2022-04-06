using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    using UnityEditor.UIElements;
    using Utilities;

    public class DSEditorWindow : EditorWindow
    {
        [MenuItem("Window/Dialogue/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }

        private void AddGraphView()
        {
            DSGraphView graphView = new DSGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            ToolbarButton saveButton = new ToolbarButton()
            {
                text = "Save Asset",
            };
            toolbar.Add(saveButton);
            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DSVariablesStyles");
        }
    }
}
