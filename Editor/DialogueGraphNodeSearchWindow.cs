using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueGraphNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView _graphView;
        private DialogueGraphEditorWindow _editorWindow;
        private Texture2D _indentationIcon;

        private struct SearchNode : IEquatable<SearchNode>
        {
            public Type Type;
            public string Entry;
            public List<SearchNode> Nodes;

            public bool Equals(SearchNode other) => Entry == other.Entry;
        }

        public void Initialize(DialogueGraphView graphView, DialogueGraphEditorWindow editorWindow)
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
            };

            List<SearchNode> searchNodes = new List<SearchNode>();
            foreach (var node in _graphView.NodeTypes)
            {
                List<SearchNode> currentSearchNodes = searchNodes;
                Queue<string> entries = new Queue<string>(node.Value.MenuName.Split('/'));

                while (entries.Count > 0)
                {
                    SearchNode newNode = new SearchNode
                    {
                        Type = node.Key,
                        Entry = entries.Dequeue()
                    };

                    if (!currentSearchNodes.Contains(newNode))
                    {
                        newNode.Nodes = new List<SearchNode>();
                        currentSearchNodes.Add(newNode);
                        currentSearchNodes = newNode.Nodes;
                    }
                    else
                    {
                        currentSearchNodes = currentSearchNodes.Find(x => x.Equals(newNode)).Nodes;
                    }
                }
            }

            void AddToTree(SearchNode searchNode, int depth = 1)
            {
                if (searchNode.Nodes.Count == 0)    // If tree entry.
                {
                    searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(searchNode.Entry, _indentationIcon))
                    {
                        level = depth,
                        userData = searchNode.Type,
                    });
                }
                else
                {
                    searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(searchNode.Entry), depth));
                    foreach (SearchNode entry in searchNode.Nodes)  // Recursive.
                        AddToTree(entry, ++depth);
                }
            }

            foreach (SearchNode searchNode in searchNodes)
                AddToTree(searchNode);         

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (_graphView.NodeTypes.ContainsKey((Type)searchTreeEntry.userData))
            {
                _graphView.AddElement(_graphView.CreateNode((Type)searchTreeEntry.userData, _graphView.GetLocalMousePosition(context.screenMousePosition - _editorWindow.position.position)));
                return true;

            }

            return false;
        }
    }
}
