using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    [Serializable, DebuggerStepThrough]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public ICollection<TKey> Keys => m_Keys ?? (ICollection<TKey>)Array.Empty<TKey>();
        ICollection IDictionary.Keys => (ICollection)Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        public ICollection<TValue> Values => m_Values ?? (ICollection<TValue>)Array.Empty<TValue>();
        ICollection IDictionary.Values => (ICollection)Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public int Count => _count;

        public bool IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        public TValue this[TKey key] 
        {
            get
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                if (_buckets is null)
                    Initialize();

                if (!_buckets!.TryGetValue(key.GetHashCode(), out int index))
                    throw new KeyNotFoundException();

                return m_Values![index];
            }
            set
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                if (_buckets is null)
                    Initialize();

                if (_buckets!.TryGetValue(key.GetHashCode(), out int index))
                {
                    m_Values![index] = value;
                }
                else
                {
                    m_Keys!.Add(key);
                    m_Values!.Add(value);
                    _buckets[key.GetHashCode()] = _count;
                    _count++;
                }
            }
        }
        object IDictionary.this[object key]
        {
            get
            {
                if (key is not TKey validKey)
                    throw new InvalidCastException();

                return this[validKey]!;
            }
            set
            {
                if (value is not TValue validValue)
                    throw new InvalidCastException();

                if (key is not TKey validKey)
                    throw new InvalidCastException();

                this[validKey] = validValue;
            }
        }

        [SerializeField]
        private List<TKey>? m_Keys;
        [SerializeField]
        private List<TValue>? m_Values;

        private Dictionary<int, int>? _buckets;

        private int _count;

        public SerializableDictionary() { }

        private void Initialize()
        {
            if (_buckets is not null)
                return;

            if (m_Keys != null && m_Values != null)
            {
                int length = m_Keys.Count;
                _buckets = new(length);

                for (int i = 0; i < length; i++)
                {
                    var key = m_Keys[i];

                    if (key == null)
                        continue;

                    _buckets![key.GetHashCode()] = i;
                }

                _count = length;
            }
            else
            {
                _buckets = new();
                m_Keys = new();
                m_Values = new();
                _count = 0;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (Count <= 0 && m_Keys != null && m_Values != null)
            {
                int keyLength = m_Keys.Count;
                int valueLength = m_Values.Count;
                if (keyLength != valueLength)
                    UnityEngine.Debug.LogError(
                        $"Serialized dictionary's data is broken! Key length:{keyLength}, value length:{valueLength}, type:{GetType()}." +
                        $"It will use the key array to reconstruct the dictionary. Any extra values will be lost.");

                Initialize();
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (_buckets is null)
                Initialize();

            if (_buckets!.ContainsKey(key.GetHashCode()))
                throw new ArgumentException();

            m_Keys!.Add(key);
            m_Values!.Add(value);
            _buckets[key.GetHashCode()] = _count;
            _count++;
        }
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        void IDictionary.Add(object key, object value)
        {
            if (key is not TKey validKey)
                throw new InvalidCastException();

            if (value is not TValue validValue)
                throw new InvalidCastException();

            Add(validKey, validValue);
        }

        public bool ContainsKey(TKey key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return _buckets?.ContainsKey(key.GetHashCode()) ?? false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (_buckets is null)
                return false;

            if (item.Key is null)
                return false;

            if (!_buckets!.TryGetValue(item.Key.GetHashCode(), out int index))
                return false;

            if (!EqualityComparer<TValue>.Default.Equals(m_Values![index], item.Value))
                return false;

            return true;
        }
        bool IDictionary.Contains(object key)
        {
            if (key is not TKey validKey)
                throw new InvalidCastException();

            return ContainsKey(validKey);
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            value = default;

            if (_buckets is null)
                return false;

            if (key is null)
                return false;

            if (!_buckets!.TryGetValue(key.GetHashCode(), out int index))
                return false;

            value = m_Values![index]!;

            return true;

        }
        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
            => TryGetValue(key, out value!);
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
            => TryGetValue(key, out value!);

        public bool Remove(TKey key)
        {
            if (_buckets is null)
                return false;

            if (key is null)
                return false;

            int hash = key.GetHashCode();

            if (!_buckets!.TryGetValue(hash, out int index))
                return false;

            m_Keys!.RemoveAt(index);
            m_Values!.RemoveAt(index);
            _buckets.Remove(hash);
            _count--;

            return true;
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            => Remove(item.Key);
        void IDictionary.Remove(object key)
        {
            if (key is not TKey validKey)
                throw new InvalidCastException();

            Remove(validKey);
        }

        public void Clear()
        {
            if (_buckets is null)
                return;

            m_Keys!.Clear();
            m_Values!.Clear();
            _buckets!.Clear();
            _count = 0;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) 
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count)
                throw new ArithmeticException();

            int count = _count;
            for (int i = 0; i < count; i++)
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(m_Keys![i], m_Values![i]);
        }
        void ICollection.CopyTo(Array array, int index)
        {
            if (array is not KeyValuePair<TKey, TValue>[] validArray)
                throw new InvalidCastException();

            ((ICollection<KeyValuePair<TKey, TValue>>)this).CopyTo(validArray, index);
        }

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);
        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private SerializableDictionary<TKey, TValue> _dictionary;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;
            private int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(SerializableDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                _dictionary = dictionary;
                _index = 0;
                _getEnumeratorRetType = getEnumeratorRetType;
                _current = default;
            }

            public bool MoveNext()
            {
                if (_index >= _dictionary.Count)
                {
                    _index = _dictionary._count + 1;
                    _current = default;
                    return false;
                }

                TKey key = _dictionary.m_Keys![_index];
                TValue value = _dictionary.m_Values![_index];
                _current = new KeyValuePair<TKey, TValue>(key, value);
                _index++;

                return true;
            }

            public KeyValuePair<TKey, TValue> Current => _current;

            public void Dispose() { }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                        throw new InvalidOperationException();

                    if (_getEnumeratorRetType == DictEntry)
                        return new DictionaryEntry(_current.Key, _current.Value);

                    return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                        throw new InvalidOperationException();

                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                        throw new InvalidOperationException();

                    return _current.Key!;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                        throw new InvalidOperationException();

                    return _current.Value!;
                }
            }
        }
    }
}
