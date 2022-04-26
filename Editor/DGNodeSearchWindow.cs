using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Celezt.DialogueSystem.Editor.Utilities;

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

        private struct NodeEntry
        {
            public Type NodeType;
            public object UserData;
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
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
            };

            //
            //  Property nodes.
            //
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Property"), 1));
            foreach (IBlackboardProperty property in _graphView.Blackboard.Properties)
            {
                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent($"{property.Name} (Property)", _indentationIcon))
                {
                    level = 2,
                    userData = new NodeEntry { NodeType = typeof(PropertyNode),  UserData = property },
                });
            }

            //
            //  Basic nodes.
            //
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Basic"), 1));
            DGBlackboard blackboard = _graphView.Blackboard;
            foreach (Type propertyType in blackboard.PropertyTypes)
            {
                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(blackboard.GetValueName(propertyType), _indentationIcon))
                {
                    level = 2,
                    userData = new NodeEntry { NodeType = typeof(BasicNode), UserData = propertyType },
                });
            }

            List<SearchNode> searchNodes = new List<SearchNode>();
            foreach (var nodeType in _graphView.NodeTypeDictionary)
            {
                if (string.IsNullOrEmpty(nodeType.Value.MenuName))
                    continue;

                List<SearchNode> currentSearchNodes = searchNodes;
                Queue<string> entries = new Queue<string>(nodeType.Value.MenuName.Trim().Split('/').Select(x => x.Trim()));

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
                        userData = new NodeEntry { NodeType = searchNode.Type }
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
            if (searchTreeEntry.userData == null)
            {
                Debug.LogWarning($"Search tree entry: {searchTreeEntry.name} has no data");
                return false;
            }

            NodeEntry entry = (NodeEntry)searchTreeEntry.userData;

            if (_graphView.NodeTypeDictionary.ContainsKey(entry.NodeType))
            {
                _graphView.AddElement(
                    _graphView.CreateNode(entry.NodeType, _graphView.GetLocalMousePosition(context.screenMousePosition - _editorWindow.position.position), Guid.NewGuid(), userData: entry.UserData));

                return true;
            }

            return false;
        }
    }
}
