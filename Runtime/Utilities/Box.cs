using System;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public interface IReadonlyBox<T>
    {
        public T Value { get; }
    }

    [Serializable]
    public struct Box<T> : IEquatable<Box<T>>, IReadonlyBox<T>, ISerializationCallbackReceiver
    {
        private static Dictionary<int, WeakReference> _boxes = new();

        public T Value
        {
            get
            {
                var weakRef = _boxes[_hash];
                if (!weakRef.IsAlive)
                    weakRef.Target = _value;

                return (T)weakRef.Target;
            }
            set => _boxes[_hash].Target = value;
        }

        T IReadonlyBox<T>.Value => Value;

        [SerializeField]
        private T _value;
        [SerializeField]
        private int _hash;

        public Box(T value)
        {
            _value = value;
            _hash = Guid.NewGuid().GetHashCode();
        }

        public static implicit operator T(Box<T> refValue) => refValue.Value;
        public static bool operator ==(Box<T> lhs, Box<T> rhs) => lhs.Value?.Equals(rhs.Value) ?? false;
        public static bool operator !=(Box<T> lhs, Box<T> rhs) => !(lhs == rhs);
        public static bool operator ==(Box<T> lhs, T rhs) => lhs.Value?.Equals(rhs) ?? false;
        public static bool operator !=(Box<T> lhs, T rhs) => !(lhs == rhs);
        public static bool operator ==(T lhs, Box<T> rhs) => lhs.Equals(rhs.Value);
        public static bool operator !=(T lhs, Box<T> rhs) => !(lhs == rhs);

        public bool Equals(Box<T> other) => other.Value?.Equals(Value) ?? false;
        public override bool Equals(object obj)
        {
            if (obj is Box<T> other)
                return Equals(other);
            else if (obj is T)
                return Value?.Equals(obj) ?? false;
            else
                return false;
        }
        override public int GetHashCode() => _hash.GetHashCode();
        public override string ToString() => Value?.ToString();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var weakRef = _boxes[_hash];
            if (weakRef.IsAlive)
                _value = (T)weakRef.Target;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_boxes.TryGetValue(_hash, out var weakRef))
            {
                if (!weakRef.IsAlive)
                   weakRef.Target = _value;                 
            }
            else
                _boxes.Add(_hash, new WeakReference(_value));
        }
    }
}