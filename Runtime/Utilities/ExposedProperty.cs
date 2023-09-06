using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem
{
    public class ExposedProperty
    {
        public ReadOnlySpan<char> Name => _name;
        public object Value
        {
            get => _value;
            set
            {
                if (!value.Equals(_value))
                {
                    OnValueChanged.Invoke(ChangeEvent<object>.GetPooled(_value, value));
                    _value = value;
                }
            }
        }

        public EventCallback<ChangeEvent<object>> OnValueChanged = delegate { };

        private string _name;
        private object _value;

        public ExposedProperty(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public T Resolve<T>() => (T)_value;
        public Type GetPropertyType() => _value.GetType();

        public override string ToString() => $"{_name}: {{{_value}}}";
    }

}
