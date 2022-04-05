using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueEditorWindow : EditorWindow
    {
        [MenuItem("Window/Dialogue/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddStyles();
        }

        private void AddGraphView()
        {
            DialogueGraphView graphView = new DialogueGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(DialogueGraphView.STYLE_PATH + "DialogueVariables.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }
}
