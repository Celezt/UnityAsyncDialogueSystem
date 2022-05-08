using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(DialogueInterpreter))]
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
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/DialogueNode");
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/ActionNode");

            //
            // Action Container
            //
            Port actionPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(ActionPortType));
            outputVerticalContainer.Add(actionPort);

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

            //
            //  Main Container
            //
            VisualElement actorContainer = new VisualElement()
            {
                name = "actor",
            };

            TextField actorIDTextField = new TextField()
            {
                value = _actorID,
            };
            actorIDTextField.RegisterValueChangedCallback(callback =>
            {
                _actorID = (callback.target as TextField).value;
                hasUnsavedChanges = true;
            });

            actorIDTextField.AddToClassList("text-field__hidden");
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

            addChoiceButton.AddToClassList("button");
            mainContainer.Insert(3, addChoiceButton);

            //
            //  Contol Container.
            //
            VisualElement textContainer = new VisualElement()
            {
                name = "text"
            };

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

            textTextField.AddToClassList("text-field");

            textFoldout.Add(textTextField);
            textContainer.Add(textFoldout);
            controlContainer.Add(textContainer);

            RefreshPorts();
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

            deleteChoiceButton.AddToClassList("button__delete");

            TextField choiceTextField = new TextField()
            {
                value = choiceData.Text,
            };
            choiceTextField.RegisterValueChangedCallback(callback =>
            {
                choiceData.Text = callback.newValue;
                hasUnsavedChanges = true;
            });


            choiceTextField.AddToClassList("text-field__choice");
            choiceTextField.AddToClassList("text-field__hidden");

            outputPort.Add(choiceTextField);
            outputPort.Add(deleteChoiceButton);
            outputContainer.Add(outputPort);
            inputContainer.Add(inputPort);
        }
    }
}
