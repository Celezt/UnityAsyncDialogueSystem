using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class DGNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DGView _graphView;
        private DGEditorWindow _editorWindow;
        private Texture2D _indentationIcon;

        private struct SearchNode : IEquatable<SearchNode>
        {
            public Type Type;
            public string Name;
            public List<SearchNode> Children;

            public bool Equals(SearchNode other) => Name == other.Name;
        }

        public void Initialize(DGView graphView, DGEditorWindow editorWindow)
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
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };

            List<SearchNode> searchNodes = new List<SearchNode>();
            foreach (var nodeType in _graphView.NodePropertiesDictionary)
            {
                List<SearchNode> currentSearchNodes = searchNodes;
                Queue<string> entries = new Queue<string>(nodeType.Value.MenuName.Split('/').Select(x => x.Trim()));

                while (entries.Count > 0)
                {
                    SearchNode newNode = new SearchNode
                    {
                        Type = nodeType.Key,
                        Name = entries.Dequeue()
                    };

                    if (!currentSearchNodes.Contains(newNode))
                    {
                        if (entries.Count > 0)
                        {
                            newNode.Children = new List<SearchNode>();
                            currentSearchNodes.Add(newNode);
                            currentSearchNodes = newNode.Children;
                        }
                        else
                        {
                            currentSearchNodes.Add(newNode);
                            break;
                        }
                    }
                    else
                    {
                        currentSearchNodes = currentSearchNodes.Find(x => x.Equals(newNode)).Children;
                    }
                }
            }

            void AddToTree(SearchNode searchNode, int depth = 1)
            {
                if (searchNode.Children == null)    // If tree entry.
                {
                    searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(searchNode.Name, _indentationIcon))
                    {
                        level = depth,
                        userData = searchNode.Type,
                    });
                }
                else
                {
                    searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(searchNode.Name), depth));
                    foreach (SearchNode entry in searchNode.Children)  // Recursive.
                        AddToTree(entry, 1 + depth);
                }
            }

            foreach (SearchNode searchNode in searchNodes)
                AddToTree(searchNode);         

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (_graphView.NodePropertiesDictionary.ContainsKey((Type)searchTreeEntry.userData))
            {
                _graphView.AddElement(_graphView.CreateNode((Type)searchTreeEntry.userData, _graphView.GetLocalMousePosition(context.screenMousePosition - _editorWindow.position.position), GUID.Generate()));
                return true;

            }

            return false;
        }
    }
}
