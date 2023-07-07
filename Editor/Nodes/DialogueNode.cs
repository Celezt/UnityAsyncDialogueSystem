using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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
        [SerializeField] private int _choiceCount;
        [SerializeField] private float _endOffset = 1;
        [SerializeField] private float _speed = 1;
        [SerializeField] private AnimationCurve _timeSpeed = AnimationCurve.Linear(0, 0, 1, 1);

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
                _actorID = (callback.target as TextField).value.Trim();
                hasUnsavedChanges = true;
            });

            actorIDTextField.AddToClassList("text-field__hidden");
            actorContainer.Add(actorIDTextField);
            mainContainer.Insert(2, actorContainer);

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
            mainContainer.Insert(3, addChoiceButton);

            //
            //  Contol Container.
            //
            FloatField endOffsetField = new FloatField()
            {
                value = _endOffset,
            };
            endOffsetField.RegisterValueChangedCallback(callback =>
            {
                var target = callback.target as FloatField;
                _endOffset = target.value;
                hasUnsavedChanges = true;
            });

            controlContainer.Add(UIElementUtility.ControlRow("End Offset", endOffsetField));

            CurveField timeSpeedField = new CurveField()
            {
                value = _timeSpeed,
            };
            timeSpeedField.RegisterValueChangedCallback(callback =>
            {
                var target = callback.target as CurveField;
                _timeSpeed = target.value;
                hasUnsavedChanges = true;
            });

            controlContainer.Add(UIElementUtility.ControlRow("Time Speed", timeSpeedField));

            FloatField durationField = new FloatField()
            {
                value = _speed,
            };
            durationField.RegisterValueChangedCallback(callback =>
            {
                var target = callback.target as FloatField;
                _speed = target.value;
                hasUnsavedChanges = true;
            });

            controlContainer.Add(UIElementUtility.ControlRow("Speed", durationField));

            //
            //  Text Container
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
                var target = callback.target as TextField;
                _text = target.value;
                hasUnsavedChanges = true;
            });

            textTextField.AddToClassList("text-field");

            textFoldout.Add(textTextField);
            textContainer.Add(textFoldout);
            controlContainer.Add(textContainer);

            RefreshPorts();
        }

        private void AddNewChoicePort()
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(ChoicePortType));
            outputPort.portName = $"{outputContainer.childCount - 1}";

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ConditionPortType));
            inputPort.portName = "Condition";

            Button deleteChoiceButton = new Button(() =>
            {
                if (outputPort.connected)
                    graphView.DeleteElements(outputPort.connections);

                if(inputPort.connected)
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
