using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Behaviour/Dialogue")]
    public class DialogueNode : CustomGraphNode
    {
        [JsonProperty] private string _actorID = "actor_id";
        [JsonProperty] private string _text = "Dialogue text.";
        [JsonProperty] private List<Choice> _choices = new List<Choice>();

        [Serializable]
        public struct Choice
        {
            public string Text;
        }

        protected override void Awake()
        {
            title = "Dialogue";

            //
            //  Input Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowType));
            inputPort.portName = "Connections";
            inputPort.portColor = FlowType.Color;
            inputContainer.Add(inputPort);

            //
            //  Output Container
            //
            Port output = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowType));
            output.portName = "Continue";
            output.portColor = FlowType.Color;
            outputContainer.Add(output);
            outputContainer.AddToClassList("dg-output__choice-container");

            //
            // Action Container
            //
            Port actionPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(ActionType));
            actionPort.portName = "";
            actionPort.portColor = ActionType.Color;

            actionPort.AddToClassList("dg-port-vertical__output");
            outputVerticalContainer.Add(actionPort);
        }

        protected override void Start()
        {
            mainContainer.AddToClassList("dg-main-container");
            extensionContainer.AddToClassList("dg-extension-container");

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
                HasUnsavedChanges = true;
            });

            actorIDTextField.AddToClassList("dg-text-field__hidden");
            actorContainer.Add(actorIDTextField);
            mainContainer.Insert(2, actorContainer);


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

        protected override object OnSerialization() => this;

        protected override void OnDeserialization(JObject loadedData)
        {
            DialogueNode node = loadedData.ToObject<DialogueNode>();

            _actorID = node._actorID;
            _text = node._text;
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
            outputPort.userData = choiceData;

            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionType));
            inputPort.portName = "Condition";
            inputPort.portColor = ConditionType.Color;

            Button deleteChoiceButton = new Button(() =>
            {
                if (outputPort.connected)
                    GraphView.DeleteElements(outputPort.connections);

                if(inputPort.connected)
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
