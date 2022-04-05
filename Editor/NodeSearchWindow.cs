using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView _graphView;
        private DialogueEditorWindow _editorWindow;
        private Texture2D _indentationIcon;

        public void Initialize(DialogueGraphView graphView, DialogueEditorWindow editorWindow)
        {
            _graphView = graphView;
            _editorWindow = editorWindow;

            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
                new SearchTreeEntry(new GUIContent("Paragraph", _indentationIcon))
                {
                    level = 2,
                    userData = typeof(ParagraphNode)
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if ((Type)SearchTreeEntry.userData == typeof(ParagraphNode))
            {
                _graphView.AddElement(_graphView.CreateNode<ParagraphNode>(_graphView.GetLocalMousePosition(context.screenMousePosition - _editorWindow.position.position)));
                return true;
            }

            return false;
        }
    }
}
