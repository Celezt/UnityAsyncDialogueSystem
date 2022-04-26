using Celezt.DialogueSystem.Editor.Utilities;
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
        public IReadOnlyList<IBlackboardProperty> Properties => _properties;
        public IReadOnlyList<Type> PropertyTypes => _propertyTypes;
        public int TypeCount => _propertyTypes.Count;

        new internal DGView graphView => _graphView;

        private readonly DGView _graphView;

        [SerializeField] private List<IBlackboardProperty> _properties = new List<IBlackboardProperty>();

        private List<Type> _propertyTypes = new List<Type>();
        private List<Type> _valueTypes = new List<Type>();
        private List<Type> _portTypes = new List<Type>();
        private List<string> _valueTypeCustomName = new List<string>();
        private Dictionary<Guid, BlackboardRow> _propertyRows = new Dictionary<Guid, BlackboardRow>();
        private Dictionary<string, IBlackboardProperty> _propertyNames = new Dictionary<string, IBlackboardProperty>();

        private BlackboardSection _section;

        public DGBlackboard(DGView graphView) : base(graphView)
        {
            _graphView = graphView;

            this.AddStyleSheet(StyleUtility.STYLE_PATH + "DGBlackboard");

            SetPosition(new Rect(10, 30, 250, 400));
            ReflectBlackboardProperties();

            subTitle = "Dialogue Graph";
            editTextRequested = OnEditName;
            addItemRequested = OnAddItem;
            moveItemRequested = OnMoveItem;
            
            
            _section = new BlackboardSection
            {
                headerVisible = false,
            };

            Add(_section);
        }

        /// <summary>
        /// Get blackboard property type from corresponding value type.
        /// </summary>
        public Type GetPropertyType(Type valueType)
        {
            int index = _valueTypes.IndexOf(valueType);

            if (index == -1)
                return null;

            return _propertyTypes[index];
        }

        /// <summary>
        /// Get value type from corresponding blackboard property type.
        /// </summary>
        public Type GetValueType(Type propertyType)
        {
            int index = _propertyTypes.IndexOf(propertyType);

            if (index == -1)
                return null;

            return _valueTypes[index];
        }

        /// <summary>
        /// Get port type from corresponding blackboard property type
        /// </summary>
        public Type GetPortType(Type propertyType)
        {
            int index = _propertyTypes.IndexOf(propertyType);

            if (index == -1)
                return null;

            return _portTypes[index];
        }

        /// <summary>
        /// Get value name from corresponding blackboard property type.
        /// </summary>
        public string GetValueName(Type propertyType)
        {
            int index = _propertyTypes.IndexOf(propertyType);

            if (index == -1)
                return null;

            string name = _valueTypeCustomName[index];

            if (string.IsNullOrEmpty(name))
                return _valueTypes[index].Name;

            return _valueTypeCustomName[index];
        }

        public void AddProperty(IBlackboardProperty property, int index = -1)
        {
            if (_propertyRows.ContainsKey(property.ID))
                return;

            property.Initialize(this);

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
                typeText = property.ValueTypeName,
                userData = property,
            };
            var row = new BlackboardRow(field, new BlackboardFieldPropertyView(property));
            row.userData = property;
            row.AddManipulator(DeleteRowManipulator(row));

            var pill = row.Q<Pill>();
            pill.RegisterCallback<MouseEnterEvent>(evt => OnMouseHoverRow(evt, property));
            pill.RegisterCallback<MouseLeaveEvent>(evt => OnMouseHoverRow(evt, property));

            if (index < 0)
                index = _propertyRows.Count;

            if (index == _propertyRows.Count)
                _section.Add(row);
            else
                _section.Insert(index, row);

            _propertyRows[property.ID] = row;

            scrollable = true;
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

                BlackboardPropertyAttribute createNodeAttribute = type.GetCustomAttribute<BlackboardPropertyAttribute>();

                _propertyTypes.Add(type);
                _valueTypes.Add(type.BaseType.GenericTypeArguments[0]);  // Get generic value type from base.
                _portTypes.Add(type.BaseType.GenericTypeArguments[1]); // Get generic value type from base.
                _valueTypeCustomName.Add(createNodeAttribute.CustomTypeName);
            }
        }

        private void OnAddItem(Blackboard blackboard)
        {
            var gm = new GenericMenu();

            foreach (Type type in _propertyTypes)
            {
                IBlackboardProperty property = (IBlackboardProperty)Activator.CreateInstance(type);
                gm.AddItem(new GUIContent(property.ValueTypeName), false, () => AddProperty(property));
            }

            gm.ShowAsContext();
        }

        private void OnEditName(Blackboard blackboard, VisualElement element, string newPropertyName)
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
                
                _graphView.EditorWindow.hasUnsavedChanges = true;
            }
        }

        private void OnMoveItem(Blackboard blackboard, int newIndex, VisualElement element)
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
                    _section.Add(_propertyRows[property.ID]);
            }
        }

        private IManipulator DeleteRowManipulator(BlackboardRow row)
        {
            return new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Delete", actionEvent =>
                {
                    var property = row.userData as IBlackboardProperty;
                    property.OnDestroy();
                    _propertyRows[property.ID].RemoveFromHierarchy();
                    _propertyRows.Remove(property.ID);
                    _properties.Remove(property);

                    if (_properties.Count == 0) // No longer scrollable if no properties exist.
                        scrollable = false;
                }));
        }

        private void OnMouseHoverRow(EventBase evt, IBlackboardProperty property)
        {
            if (evt.eventTypeId == MouseEnterEvent.TypeId())
            {
                foreach (Node node in _graphView.nodes)
                {

                }
            }
        }
    }
}
