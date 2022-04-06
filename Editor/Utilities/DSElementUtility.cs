using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class DSElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        public static Foldout CreateFoldout(string title, bool isCollapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !isCollapsed
            };

            return foldout;
        }

        public static Port CreatePort(this Node node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;

            return port;
        }

        public static TextField CreateTextField(string value = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value
            };

            if (onValueChanged is { })
                textField.RegisterValueChangedCallback(onValueChanged);

            return textField;
        }

        public static TextField CreateTextArea(string value = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, onValueChanged);
            textArea.multiline = true;

            return textArea;
        }
    }
}
