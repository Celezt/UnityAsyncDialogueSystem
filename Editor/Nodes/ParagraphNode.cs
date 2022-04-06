using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    using Utilities;

    public class ParagraphNode : DSNode
    {
        public string ParagraphName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }

        public override void Initialize(Vector2 position)
        {
            ParagraphName = "Paragraph Name";
            Choices = new List<string>() { "New Choice" };
            Text = "Text.";

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public override void Draw()
        {
            //
            //  Title container
            //
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(ParagraphName);
            dialogueNameTextField.AddToClassList("ds-node__text-field");
            dialogueNameTextField.AddToClassList("ds-node__filename-text-field");
            dialogueNameTextField.AddToClassList("ds-node__text-field__hidden");
            titleContainer.Insert(0, dialogueNameTextField);

            //
            //  Main Container
            //
            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
            {
                Port choicePort = CreateChoicePort("New Choice");
                Choices.Add("New Choice");
                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            //
            //  Input Container
            //
            Port inputPort = this.CreatePort("Connections", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            //
            // Output Container
            //
            foreach (string choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }

            //
            //  Extensions Container
            //
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Sequence");
            TextField textTextField = DSElementUtility.CreateTextArea(Text);

            textTextField.AddToClassList("ds-node__text-field");
            textTextField.AddToClassList("ds-node__quote-text-field");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }

        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();

            Button deleteChoiceButton = DSElementUtility.CreateButton("X");

            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DSElementUtility.CreateTextField(choice);

            choiceTextField.AddToClassList("ds-node__text-field");
            choiceTextField.AddToClassList("ds-node__choice-text-field");
            choiceTextField.AddToClassList("ds-node__text-field__hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        }
    }
}
