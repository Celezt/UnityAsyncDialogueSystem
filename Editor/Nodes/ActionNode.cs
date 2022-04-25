using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Behaviour/Action", "Action")]
    public class ActionNode : DGNode
    {
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
            Port actionPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(ActionPortType));
            actionPort.portName = "";

            actionPort.AddToClassList("dg-port-vertical__input");
            inputVerticalContainer.Insert(0, actionPort);
            mainContainer.AddToClassList("dg-main-container");

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
            mainContainer.Insert(2, addChoiceButton);
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

                if (inputPort.connected)
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
