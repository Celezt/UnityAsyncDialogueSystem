using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Value Asset", menuName = "Dialogue/Assets/Value Asset")]
    public class ValueAsset : ProcessAsset, ISerializationCallbackReceiver
    {
        public object Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    if (value is double or int)
                        _value = Convert.ToSingle(value);
                    else
                        _value = value;

                    IsDirty();
                }
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
