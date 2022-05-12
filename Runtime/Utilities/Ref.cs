using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public class Ref<T> : IEquatable<Ref<T>>
    {
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        [SerializeField]
        private T _value;

        public Ref(T value) => _value = value;
        public Ref() => _value = default(T);

        public static implicit operator T(Ref<T> refValue) => refValue.Value;
        public static bool operator ==(Ref<T> lhs, Ref<T> rhs) => lhs.Value?.Equals(rhs.Value) ?? false;
        public static bool operator !=(Ref<T> lhs, Ref<T> rhs) => !(lhs == rhs);
        public static bool operator ==(Ref<T> lhs, T rhs) => lhs.Value?.Equals(rhs) ?? false;
        public static bool operator !=(Ref<T> lhs, T rhs) => !(lhs == rhs);
        public static bool operator ==(T lhs, Ref<T> rhs) => lhs.Equals(rhs.Value);
        public static bool operator !=(T lhs, Ref<T> rhs) => !(lhs == rhs);

        public bool Equals(Ref<T> other) => other.Value?.Equals(Value) ?? false;
        public override bool Equals(object obj)
        {
            if (obj is Ref<T> other)
                return Equals(other);
            else if (obj is T)
                return Value?.Equals(obj) ?? false;
            else
                return false;
        }
        override public int GetHashCode() => base.GetHashCode();
        public override string ToString() => Value?.ToString() ?? "";
    }
}