using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Behaviour/Action")]
    public class ActionNode : CustomGraphNode
    {
        [JsonProperty] private List<Choice> _choices = new List<Choice>();

        [Serializable]
        public struct Choice
        {
            public string Text;
        }

        protected override void Awake()
        {
            title = "Action";

            //
            // Action Container
            //
            Port actionPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(ActionType));
            actionPort.portName = "";
            actionPort.portColor = ActionType.Color;

            actionPort.AddToClassList("dg-port-vertical__input");
            inputVerticalContainer.Insert(0, actionPort);
            mainContainer.AddToClassList("dg-main-container");
        }

        protected override void Start()
        {
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

        protected override object OnSerialization() => this;

        protected override void OnDeserialization(JObject loadedData)
        {
            ActionNode node = loadedData.ToObject<ActionNode>();

            _choices = node._choices;

            foreach (var choice in _choices)
            {
                AddNewChoicePort(choice);
            }
        }

        private void AddNewChoicePort(Choice choiceData)
        {
            HasUnsavedChanges = true;

            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowType));
            outputPort.portName = "";
            outputPort.portColor = FlowType.Color;

            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionType));
            inputPort.portName = "Condition";
            inputPort.portColor = ConditionType.Color;

            Button deleteChoiceButton = new Button(() =>
            {
                if (outputPort.connected)
                    GraphView.DeleteElements(outputPort.connections);

                if (inputPort.connected)
                    GraphView.DeleteElements(inputPort.connections);

                HasUnsavedChanges = true;

                _choices.Remove(choiceData);
                GraphView.RemoveElement(outputPort);
                GraphView.RemoveElement(inputPort);
            });

            deleteChoiceButton.AddToClassList("dg-button__delete");

            TextField choiceTextField = new TextField()
            {
                value = choiceData.Text,
            };
            choiceTextField.RegisterValueChangedCallback(callback =>
            {
                choiceData.Text = callback.newValue;
                HasUnsavedChanges = true;
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
