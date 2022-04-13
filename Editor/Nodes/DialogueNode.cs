using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{

    [CreateNode("Behaviour/Dialogue Node"), JsonObject(MemberSerialization.OptIn)]
    public class DialogueNode : CustomGraphNode
    {
        [JsonProperty] private string _actorID = "actor_id";
        [JsonProperty] private string _text = "Dialogue text.";
        [JsonProperty] private List<Choice> _choices = new List<Choice>();

        [Serializable]
        public struct Choice
        {
            public string ID;
            public string Text;
        }

        protected override void Start()
        {
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            //
            //  Title container
            //
            TextField actorIDTextField = new TextField()
            {
                value = _actorID,
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
                _choices.Add(new Choice { Text = "New Choice" });
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
                value = _text,
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
                        CustomGraphNode nextNode = (CustomGraphNode)edge.input.node;
                        Choice choice = (Choice)edge.output.userData;
                        choice.ID = nextNode.Guid.ToString();
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

        protected override object OnSaveData() => this;

        protected override void OnLoadData(object loadedData)
        {
            DialogueNode node = JsonConvert.DeserializeObject<DialogueNode>(loadedData.ToString());
            _actorID = node._actorID;
            _text = node._text;
            _choices = node._choices;

            foreach (var choice in _choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
        }

        protected override void AfterLoad()
        {
            if (_choices.Count == 0)
            {
                Choice choice = new Choice { Text = "New Choice" };
                _choices.Add(choice);

                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
        }

        private Port CreateChoicePort(Choice choiceData)
        {
            Port choicePort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            choicePort.portName = "";
            choicePort.userData = choiceData;

            Button deleteChoiceButton = new Button(() =>
            {
                if (_choices.Count == 1)
                    return;

                if (choicePort.connected)
                    GraphView.DeleteElements(choicePort.connections);

                _choices.Remove(choiceData);
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
