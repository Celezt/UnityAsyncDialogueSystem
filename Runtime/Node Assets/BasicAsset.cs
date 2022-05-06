using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Basic Asset", menuName = "Dialogue/Assets/Basic Asset")]
    public class BasicAsset : NodeAsset, ISerializationCallbackReceiver
    {
        public object Value
        {
            get => _value;
            set
            {
                if (value is double or int)
                    _value = Convert.ToSingle(value);
                else 
                    _value = value;
            }
        }
        private object _value = default(float);

        [SerializeField, HideInInspector]
        private string _valueSerilaized;

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrWhiteSpace(_valueSerilaized))
                return;

            Value = JsonConvert.DeserializeObject(_valueSerilaized);
        }

        public void OnBeforeSerialize()
        {
            if (_value == null)
                return;

            _valueSerilaized = JsonConvert.SerializeObject(_value);
        }

        protected override void OnCreateAsset(IReadOnlyDictionary<string, object> values)
        {
            if (values != null)
                Value = values["_value"];
        }

        protected override object Process(object[] inputs, int outputIndex)
        {
            return _value;
        }
    }
}
