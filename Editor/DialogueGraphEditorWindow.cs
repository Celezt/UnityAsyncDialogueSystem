using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueGraphEditorWindow : EditorWindow
    {
        [MenuItem("Window/Dialogue/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueGraphEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
        }

        private void AddGraphView()
        {
            DialogueGraph graphView = new DialogueGraph();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
    }
}
