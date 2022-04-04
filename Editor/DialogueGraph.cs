using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueGraph : GraphView
    {
        private const string STYLE_PATH = "Packages/com.celezt.asyncdialogue/Editor/Resources/Styles/";

        public DialogueGraph()
        {
            AddGridBackground();
            AddStyles();
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_PATH + "DialogueGraphView.uss");
            styleSheets.Add(styleSheet);

        }
    }
}
