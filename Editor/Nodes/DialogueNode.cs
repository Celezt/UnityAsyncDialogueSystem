using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{

    [CreateNode("Behaviour/Dialogue Node")]
    public class DialogueNode : DialogueGraphNode
    {
        public string ActorID { get; set; } = "actor_id";
        public List<Choice> Choices { get; set; } = new List<Choice>() { new Choice { Text = "New Choice" } };
        public string Text { get; set; } = "Dialogue text.";

        [Serializable]
        public struct Choice
        {
            public string ID;
            public string Text;
        }

        [Serializable]
        public struct SaveData
        {
            public string ActorID;
            public List<Choice> Choices;
            public string Text;
        }

        public DialogueNode(GraphView graphView, Vector2 position) : base(graphView, position)
        {
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            //
            //  Title container
            //
            TextField actorIDTextField = new TextField()
            {
                value = ActorID,
            };
            actorIDTextField.AddToClassList("ds-node__text-field");
            actorIDTextField.AddToClassList("ds-node__filename-text-field");
            actorIDTextField.AddToClassList("ds-node__text-field__hidden");
            titleContainer.Insert(0, actorIDTextField);

            //
            //  Main Container
            //
            Button addChoiceButton = new Button(() =>
            {
                Port choicePort = CreateChoicePort(new Choice { Text = "New Choice" });
                Choices.Add(new Choice { Text = "New Choice" });
                outputContainer.Add(choicePort);
            })
            {
                text = "Add Choice",
            };

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            //
            //  Input Container
            //

            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Connections";
            inputContainer.Add(inputPort);

            //
            // Output Container
            //
            foreach (Choice choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }

            //
            //  Extensions Container
            //
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = new Foldout()
            {
                text = "Sequence",
                value = true,
            };

            TextField textTextField = new TextField()
            {
                value = Text,
            };
            textTextField.multiline = true;

            textTextField.AddToClassList("ds-node__text-field");
            textTextField.AddToClassList("ds-node__quote-text-field");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }

        protected override void OnEdgeChanged(Edge edge, EdgeState state)
        {
            switch (state)
            {
                case EdgeState.Created | EdgeState.Output:
                    {
                        DialogueGraphNode nextNode = (DialogueGraphNode)edge.input.node;
                        Choice choice = (Choice)edge.output.userData;
                        choice.ID = nextNode.ID.ToString();
                        edge.output.userData = choice;
                        break;
                    }
                case EdgeState.Removed | EdgeState.Output:
                    {
                        Choice choice = (Choice)edge.output.userData;
                        choice.ID = "";
                        break;
                    }
            }
        }

        protected override object CustomSaveData()
        {
            return new SaveData{ ActorID = ActorID, Choices = Choices, Text = Text};
        }

        private Port CreateChoicePort(Choice choiceData)
        {
            Port choicePort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            choicePort.portName = "";
            choicePort.userData = choiceData;

            Button deleteChoiceButton = new Button(() =>
            {
                if (Choices.Count == 1)
                    return;

                if (choicePort.connected)
                    GraphView.DeleteElements(choicePort.connections);

                Choices.Remove(choiceData);
                GraphView.RemoveElement(choicePort);
            })
            {
                text = "X"
            };

            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = new TextField()
            {
                value = choiceData.Text,
                multiline = true,
            };
            choiceTextField.RegisterValueChangedCallback(callback =>
            {
                choiceData.Text = callback.newValue;
            });      
            
            choiceTextField.AddToClassList("ds-node__text-field");
            choiceTextField.AddToClassList("ds-node__choice-text-field");
            choiceTextField.AddToClassList("ds-node__text-field__hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        }
    }
}
