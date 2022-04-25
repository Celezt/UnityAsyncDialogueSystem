using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Behaviour/Dialogue", "Dialogue")]
    public class DialogueNode : DGNode
    {
        [SerializeField] private string _actorID = "actor_id";
        [SerializeField] private string _text = "Dialogue text.";
        [SerializeField] private List<Choice> _choices = new List<Choice>();

        [Serializable]
        public struct Choice
        {
            public string Text;
        }

        protected override void Awake()
        {
            //
            // Action Container
            //
            Port actionPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(ActionPortType));

            actionPort.AddToClassList("dg-port-vertical__output");
            outputVerticalContainer.Add(actionPort);

            mainContainer.AddToClassList("dg-main-container");
            extensionContainer.AddToClassList("dg-extension-container");

            //
            //  Input Container
            //
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowPortType));
            inputPort.portName = "Connections";
            inputContainer.Add(inputPort);

            //
            //  Output Container
            //
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));
            output.portName = "Continue";
            outputContainer.Add(output);
            outputContainer.AddToClassList("dg-output__choice-container");

            //
            //  Main Container
            //
            VisualElement actorContainer = new VisualElement();
            actorContainer.AddToClassList("dg-padding-8-container");

            TextField actorIDTextField = new TextField()
            {
                value = _actorID,
            };
            actorIDTextField.RegisterValueChangedCallback(callback =>
            {
                _actorID = (callback.target as TextField).value;
                hasUnsavedChanges = true;
            });

            actorIDTextField.AddToClassList("dg-text-field__hidden");
            actorContainer.Add(actorIDTextField);
            mainContainer.Insert(2, actorContainer);

            foreach (Choice choice in _choices)
                AddNewChoicePort(choice);

            Button addChoiceButton = new Button(() =>
            {
                AddNewChoicePort(new Choice { Text = "New Choice" });
                _choices.Add(new Choice { Text = "New Choice" });
            })
            {
                text = "Add Choice",
            };

            addChoiceButton.AddToClassList("dg-button");
            mainContainer.Insert(3, addChoiceButton);

            //
            //  Extensions Container
            //
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("dg-custom-data-container");

            Foldout textFoldout = new Foldout()
            {
                text = "Text",
                value = true,
            };

            TextField textTextField = new TextField()
            {
                value = _text,
                multiline = true,
            };
            textTextField.RegisterValueChangedCallback(callback =>
            {
                TextField target = callback.target as TextField;
                _text = target.value;
            });

            textTextField.AddToClassList("dg-text-field");
            textTextField.AddToClassList("dg-quote-text-field");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }

        private void AddNewChoicePort(Choice choiceData)
        {
            hasUnsavedChanges = true;

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionPortType));
            inputPort.portName = "Condition";

            Button deleteChoiceButton = new Button(() =>
            {
                if (outputPort.connected)
                    graphView.DeleteElements(outputPort.connections);

                if(inputPort.connected)
                    graphView.DeleteElements(inputPort.connections);

                hasUnsavedChanges = true;

                _choices.Remove(choiceData);
                graphView.RemoveElement(outputPort);
                graphView.RemoveElement(inputPort);
            });

            deleteChoiceButton.AddToClassList("dg-button__delete");

            TextField choiceTextField = new TextField()
            {
                value = choiceData.Text,
            };
            choiceTextField.RegisterValueChangedCallback(callback =>
            {
                choiceData.Text = callback.newValue;
                hasUnsavedChanges = true;
            });


            choiceTextField.AddToClassList("dg-text-field__choice");
            choiceTextField.AddToClassList("dg-text-field__hidden");
            outputPort.AddToClassList("dg-port__choice-container");

            outputPort.Add(choiceTextField);
            outputPort.Add(deleteChoiceButton);
            outputContainer.Add(outputPort);
            inputContainer.Add(inputPort);
        }
    }
}
