using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class DGBlackboard : Blackboard
    {
        private readonly DGView _graphView;

        private List<Type> _propertyTypes = new List<Type>();
        private List<IBlackboardProperty> _properties = new List<IBlackboardProperty>();
        private Dictionary<Guid, BlackboardRow> _propertyRows = new Dictionary<Guid, BlackboardRow>();
        private Dictionary<string, IBlackboardProperty> _propertyNames = new Dictionary<string, IBlackboardProperty>();

        private BlackboardSection _section;

        public DGBlackboard(DGView graphView) : base(graphView)
        {
            _graphView = graphView;

            SetPosition(new Rect(10, 30, 180, 350));
            ReflectBlackboardProperties();

            scrollable = true;
            subTitle = "Dialogue Graph";
            editTextRequested = EditTextRequested;
            addItemRequested = AddItemRequested;
            moveItemRequested = MoveItemRequested;

            _section = new BlackboardSection
            {
                headerVisible = false,
            };
            foreach (var property in _properties)
                AddProperty(property);

            Add(_section);
        }

        public void AddProperty(IBlackboardProperty property, int index = -1)
        {
            if (_propertyRows.ContainsKey(property.GUID))
                return;

            // Rename if duplicate.
            {
                string newPropertyName = property.Name;
                int duplicateIndex = 0;
                while (_propertyNames.ContainsKey(newPropertyName))
                    newPropertyName = $"{property.Name} {++duplicateIndex}";
                property.Name = newPropertyName;
            }

            _propertyNames.Add(property.Name, property);
            _properties.Add(property);

            var field = new BlackboardField
            {
                text = property.Name,
                typeText = property.PropertyTypeName,
                userData = property,
            };
            var row = new BlackboardRow(field, new BlackboardFieldPropertyView(property));
            row.userData = property;

            if (index < 0)
                index = _propertyRows.Count;

            if (index == _propertyRows.Count)
                _section.Add(row);
            else
                _section.Insert(index, row);

            _propertyRows[property.GUID] = row;
        }

        private void ReflectBlackboardProperties()
        {
            foreach (Type type in ReflectionUtility.GetTypesWithAttribute<BlackboardPropertyAttribute>(AppDomain.CurrentDomain))
            {
                if (!typeof(IBlackboardProperty).IsAssignableFrom(type))
                {
                    Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(IBlackboardProperty)}");
                    continue;
                }

                _propertyTypes.Add(type);
            }
        }

        private void AddItemRequested(Blackboard blackboard)
        {
            var gm = new GenericMenu();

            foreach (Type type in _propertyTypes)
            {
                IBlackboardProperty property = (IBlackboardProperty)Activator.CreateInstance(type);
                gm.AddItem(new GUIContent(property.PropertyTypeName), false, () => AddProperty(property));
            }

            gm.ShowAsContext();
        }

        private void EditTextRequested(Blackboard blackboard, VisualElement element, string newPropertyName)
        {
            if (string.IsNullOrEmpty(newPropertyName))
                return;

            newPropertyName = newPropertyName.Trim();

            var field = element as BlackboardField;

            string oldPropertyName = field.text;

            if (!_propertyNames.ContainsKey(newPropertyName))
            {
                IBlackboardProperty property = _propertyNames[oldPropertyName];
                property.Name = newPropertyName;
                field.text = newPropertyName;

                _propertyNames.Remove(oldPropertyName);
                _propertyNames.Add(newPropertyName, property);
            }
        }

        private void MoveItemRequested(Blackboard blackboard, int newIndex, VisualElement element)
        {
            // Reorder properties.
            {
                IBlackboardProperty property = element.userData as IBlackboardProperty;

                if (property == null)
                    return;

                if (newIndex > _properties.Count || newIndex < 0)
                    throw new ArgumentException("New index is not within properties list.");

                int currentIndex = _properties.IndexOf(property);
                if (currentIndex == -1)
                    throw new ArgumentException("Property is not in graph.");

                if (newIndex == currentIndex)
                    return;

                _properties.RemoveAt(currentIndex);

                if (newIndex > currentIndex)
                    newIndex--;

                if (newIndex == _properties.Count) // is last.
                    _properties.Add(property);
                else
                    _properties.Insert(newIndex, property);
            }

            // Reorder rows.
            {
                foreach (var row in _propertyRows.Values)   // Remove all rows.
                    row.RemoveFromHierarchy();

                foreach (var property in _properties)       // Add all rows in the new order.
                    _section.Add(_propertyRows[property.GUID]);
            }
        }
    }
}