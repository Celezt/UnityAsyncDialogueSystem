using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

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

        public static Box<T> Empty = new Box<T>();
        public bool IsEmpty => _hash == 0;

        public T Value
        {
            get
            {
                Initialize();

                return GetValue(_boxed!);
            }
            set
            {
                Initialize();

                if (_boxed is ValueObject valueObject)
                    valueObject.Value = value;
                else
                {
                    _boxed = value;
                    _boxes[_hash].Target = _boxed;
                }
            }
        }

        T IReadonlyBox<T>.Value => Value;

        [SerializeField]
        private T _value;
        [SerializeField]
        private int _hash;

        private object? _boxed;

        private class ValueObject
        {
            public T Value
            {
                get => _value;
                set => _value = value;
            }

            private T _value;

            public ValueObject(T value) => _value = value;
        }

        public Box(T value)
        {
            _value = value;
            _hash = Guid.NewGuid().GetHashCode();
            _boxed = null;
        }

        public static implicit operator T(Box<T> refValue) => refValue.Value;
        public static bool operator ==(Box<T> lhs, Box<T> rhs) => lhs.Value?.Equals(rhs.Value) ?? rhs.Value == null;
        public static bool operator !=(Box<T> lhs, Box<T> rhs) => !(lhs == rhs);
        public static bool operator ==(Box<T> lhs, T rhs) => lhs.Value?.Equals(rhs) ?? rhs == null;
        public static bool operator !=(Box<T> lhs, T rhs) => !(lhs == rhs);
        public static bool operator ==(T lhs, Box<T> rhs) => lhs?.Equals(rhs.Value) ?? rhs.Value == null;
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
        override public int GetHashCode()
        {
            if (_hash == 0)
                _hash = Guid.NewGuid().GetHashCode();

            return _hash.GetHashCode();
        }

        public override string ToString() => Value?.ToString() ?? string.Empty;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_hash == 0)
                _hash = Guid.NewGuid().GetHashCode();

            if (_boxed != null)
                _value = GetValue(_boxed);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_hash == 0)
                _hash = Guid.NewGuid().GetHashCode();

            if (_boxed == null)
            {
                if (_boxes.TryGetValue(_hash, out var weakRef))
                {
                    if (weakRef.IsAlive)
                        _boxed = weakRef.Target;
                    else
                    {
                        _boxed = BoxValue(_value);
                        weakRef.Target = _boxed;
                    }
                }
                else
                {
                    _boxed = BoxValue(_value);
                    _boxes.Add(_hash, new WeakReference(_boxed));
                }
            }
        }

        /// <summary>
        /// Places the value on the heap if it is a value type.
        /// </summary>
        private static object? BoxValue(T value) => typeof(T).IsValueType ? new ValueObject(value) : value;

        private static T GetValue(object boxed)
        {
            if (boxed is ValueObject valueObject)
                return valueObject.Value;
            else
                return (T)boxed;
        }
    }
}