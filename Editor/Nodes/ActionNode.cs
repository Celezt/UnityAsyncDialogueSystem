using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(ActionInterpreter))]
    [CreateNode("Behaviour/Action", "Action")]
    public class ActionNode : DGNode
    {
        [SerializeField] private int _choiceCount;

        public class Choice
        {
            public string Text;
        }

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/ActionNode");

            //
            // Action Container
            //
            Port actionPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(ActionPortType));

            inputVerticalContainer.Insert(0, actionPort);

            for (int i = 0; i < _choiceCount; i++)
                AddNewChoicePort();

            Button addChoiceButton = new Button(() =>
            {
                hasUnsavedChanges = true;

                AddNewChoicePort();
                _choiceCount++;
            })
            {
                text = "Add Choice",
            };

            addChoiceButton.AddToClassList("button");
            mainContainer.Insert(2, addChoiceButton);
        }

        private void AddNewChoicePort()
        {
            hasUnsavedChanges = true;

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(ChoicePortType));
            outputPort.portName = $"{outputContainer.childCount}";

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionPortType));
            inputPort.portName = "Condition";

            Button deleteChoiceButton = new Button(() =>
            {
                if (outputPort.connected)
                    graphView.DeleteElements(outputPort.connections);

                if (inputPort.connected)
                    graphView.DeleteElements(inputPort.connections);

                hasUnsavedChanges = true;

                _choiceCount--;
                graphView.RemoveElement(outputPort);
                graphView.RemoveElement(inputPort);

                int count = 0;
                foreach (var element in outputContainer.Children().Skip(1))
                    if (element is Port port)
                        port.portName = $"{count++}";
            });

            deleteChoiceButton.AddToClassList("button__delete");

            outputPort.Add(deleteChoiceButton);
            outputContainer.Add(outputPort);
            inputContainer.Add(inputPort);
        }
    }
}
