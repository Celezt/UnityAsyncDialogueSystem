using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem
{
    public struct ExposedProperty : IEquatable<ExposedProperty>
    {
        public Guid ID => _id;
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

        public EventCallback<ChangeEvent<object>> OnValueChanged;

        private readonly Guid _id;

        private string _name;
        private object _value;

        public ExposedProperty(string name, object value) : this(name, value, Guid.NewGuid()) { }
        public ExposedProperty(string name, object value, Guid id)
        {
            _id = id;
            _name = name;
            _value = value;
            OnValueChanged = delegate { };
        }

        public T Resolve<T>() => (T)_value;
        public Type GetPropertyType() => _value.GetType();

        public bool Equals(ExposedProperty other) => _id == other._id;
        public override bool Equals(object obj) => obj is ExposedProperty exposed && Equals(exposed);
        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => $"{_name}: {{{_value}}}";

        public static bool operator ==(ExposedProperty lhs, ExposedProperty rhs) => lhs.Value.Equals(rhs.Value);
        public static bool operator !=(ExposedProperty lhs, ExposedProperty rhs) => !(lhs == rhs);
    }

}
