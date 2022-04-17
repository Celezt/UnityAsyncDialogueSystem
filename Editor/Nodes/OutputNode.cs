using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Connection/Output"), JsonObject(MemberSerialization.OptIn)]
    public class OutputNode : CustomGraphNode
    {
        [JsonProperty] private string _id = "ID";

        protected override void Awake()
        {
            title = "Output";
        }

        protected override void Start()
        {
            //
            // Output Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "";

            TextField inputTextField = new TextField()
            {
                value = _id,
            };
            inputTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
            });

            inputTextField.AddToClassList("dg-text-field__hidden");

            inputPort.Add(inputTextField);
            inputContainer.Add(inputPort);
        }

        protected override object OnSerialization() => this;

        protected override void OnDeserialization(JObject loadedData)
        {
            var data = loadedData.ToObject<OutputNode>();
            _id = data._id;
        }
    }
}
