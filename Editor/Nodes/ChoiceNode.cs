using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(ChoiceInterpreter))]
    [CreateNode("Behaviour/Choice", "Choice")]
    public class ChoiceNode : DGNode
    {
        [SerializeField] private string _text = "Choice text.";

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/DialogueNode");

            //
            //  Input Container
            //
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ChoicePortType));
            inputPort.portName = "Connections";
            inputContainer.Add(inputPort);

            //
            //  Output Container
            //
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));
            output.portName = "Continue";
            outputContainer.Add(output);

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
        }
    }
}
