using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Connection/Input"), JsonObject(MemberSerialization.OptIn)]
    public class InputNode : CustomGraphNode
    {
        [JsonProperty] private string _id = "ID";

        protected override void Awake()
        {
            title = "Input";
            mainContainer.AddToClassList("dg-main-container");
            mainContainer.AddToClassList("dg-connection-container");
        }

        protected override void Start()
        {
            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueType));
            outputPort.portName = "";
            outputPort.portColor = DialogueType.Color;

            TextField outputTextField = new TextField()
            {
                value = _id,
            };
            outputTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
                HasUnsavedChanges = true;
            });

            outputTextField.AddToClassList("dg-text-field__hidden");
            outputTextField.AddToClassList("dg-text-field__wide");

            outputPort.Add(outputTextField);
            outputContainer.Add(outputPort);
        }

        protected override object OnSerialization() => this;

        protected override void OnDeserialization(JObject loadedData)
        {
            var data = loadedData.ToObject<InputNode>();
            _id = data._id;
        }
    }
}
