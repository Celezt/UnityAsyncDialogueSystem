using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueNode : Node, INode
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }

        public void Initialize(Vector2 position)
        {
            DialogueName = "Dialogue Name";
            Choices = new List<string>() { "New Choice" };
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public void Draw()
        {
            TextField dialogueNameTextField = new TextField()
            {
                value = DialogueName
            };
            //
            //  Title container
            //
            dialogueNameTextField.AddToClassList("ds-node__text-field");
            dialogueNameTextField.AddToClassList("ds-node__filename-text-field");
            dialogueNameTextField.AddToClassList("ds-node__text-field__hidden");

            titleContainer.Insert(0, dialogueNameTextField);
            

            //
            //  Main Container
            //
            Button addChoiceButton = new Button()
            {
                text = "Add Choice"
            };

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            //
            //  Input Container
            //
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Parents";
            inputContainer.Add(inputPort);

            //
            // Output Container
            //
            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                choicePort.portName = "";

                Button deleteChoiceButton = new Button()
                {
                    text = "X"
                };

                deleteChoiceButton.AddToClassList("ds-node__button");

                TextField choiceTextField = new TextField()
                {
                    value = Text
                };

                choiceTextField.AddToClassList("ds-node__text-field");
                choiceTextField.AddToClassList("ds-node__choice-text-field");
                choiceTextField.AddToClassList("ds-node__text-field__hidden");

                choicePort.Add(choiceTextField);
                choicePort.Add(deleteChoiceButton);
                outputContainer.Add(choicePort); 
            }

            //
            //  Extensions Container
            //
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = new Foldout()
            {
                text = "Sequence"
            };

            TextField textTextField = new TextField()
            {
                value = Text
            };

            textTextField.AddToClassList("ds-node__text-field");
            textTextField.AddToClassList("ds-node__quote-text-field");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}
