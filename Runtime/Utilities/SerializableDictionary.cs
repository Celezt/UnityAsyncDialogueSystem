using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    // https://github.com/Prastiwar/UnitySerializedDictionary
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        private int[]? _buckets;
        private Entry[]? _entries;

        private int _count;
        private int _freeList;
        private int _freeCount;
        private int _version;
        private IEqualityComparer<TKey>? _comparer;
        private KeyCollection? _keys;
        private ValueCollection? _values;
        private object? _syncRoot;

        [SerializeField]
        private TKey[]? m_keys;
        [SerializeField]
        private TValue[]? m_values;

        [Serializable]
        private struct Entry
        {
            public int HashCode;    // Lower 31 bits of hash code, -1 if unused
            public int Next;        // Index of next entry, -1 if last
            public TKey Key;           // Key of entry
            public TValue Value;         // Value of entry
        }

        public SerializableDictionary() : this(0, null) { }

        public SerializableDictionary(int capacity) : this(capacity, null) { }

        public SerializableDictionary(IEqualityComparer<TKey>? comparer) : this(0, comparer) { }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey>? comparer)
        {
            if (capacity < 0) 
                throw new ArgumentOutOfRangeException("capacity");

            if (capacity > 0) 
                Initialize(capacity);

            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer) :
            this(dictionary != null ? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                Add(pair.Key, pair.Value);
        }

        public IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;

        public int Count => _count - _freeCount;

        public KeyCollection Keys => _keys ??= new KeyCollection(this);
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection IDictionary.Keys => Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        public ValueCollection Values => _values ??= new ValueCollection(this);
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        ICollection IDictionary.Values => Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public TValue this[TKey key]
        {
            get
            {
                int i = FindEntry(key);

                if (i < 0) 
                    throw new KeyNotFoundException();

                    return _entries[i].Value;

            }
            set
            {
                Insert(key, value, false);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_keys = Keys.ToArray();
            m_values = Values.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (Count == 0 && m_keys != null && m_values != null)
            {
                int length = m_keys.Length;
                int valueLength = m_values.Length;
                if (length != valueLength)
                {
                    UnityEngine.Debug.LogError(
                        $"{typeof(SerializableDictionary<,>).Name} data is broken! key length:{length} value length:{valueLength} " +
                        $"{typeof(SerializableDictionary<,>).Name} type:{GetType()}");
                }

                Clear();
                for (int i = 0; i < length; i++)
                {
                    this[m_keys[i]] = valueLength > i ? m_values[i] : default(TValue);
                }

                m_keys = null;
                m_values = null;
            }
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].Value, keyValuePair.Value))
            {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].Value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (_count > 0)
            {
                for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
                Array.Clear(_entries, 0, _count);
                _freeList = -1;
                _count = 0;
                _freeCount = 0;
                _version++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_entries[i].HashCode >= 0 && _entries[i].Value == null) return true;
                }
            }
            else
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                for (int i = 0; i < _count; i++)
                {
                    if (_entries[i].HashCode >= 0 && c.Equals(_entries[i].Value, value)) return true;
                }
            }
            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException();
            }

            int count = this._count;
            Entry[] entries = this._entries;
            for (int i = 0; i < count; i++)
            {
                if (entries[i].HashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(entries[i].Key, entries[i].Value);
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        private int FindEntry(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (_buckets != null)
            {
                int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && _comparer.Equals(_entries[i].Key, key)) return i;
                }
            }
            return -1;
        }

        private void Initialize(int capacity)
        {
            int size = HashHelpers.GetPrime(capacity);
            _buckets = new int[size];
            for (int i = 0; i < _buckets.Length; i++) 
                _buckets[i] = -1;
            _entries = new Entry[size];
            _freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (_buckets == null) Initialize(0);
            int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % _buckets.Length;

#if FEATURE_RANDOMIZED_STRING_HASHING
            int collisionCount = 0;
#endif

            for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
            {
                if (_entries[i].HashCode == hashCode && _comparer.Equals(_entries[i].Key, key))
                {
                    if (add)
                    {
                        throw new ArgumentException();
                    }
                    _entries[i].Value = value;
                    _version++;
                    return;
                }
            }
            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = _entries[index].Next;
                _freeCount--;
            }
            else
            {
                if (_count == _entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % _buckets.Length;
                }
                index = _count;
                _count++;
            }

            _entries[index].HashCode = hashCode;
            _entries[index].Next = _buckets[targetBucket];
            _entries[index].Key = key;
            _entries[index].Value = value;
            _buckets[targetBucket] = index;
            _version++;
        }

        private void Resize()
        {
            Resize(HashHelpers.ExpandPrime(_count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            //Contract.Assert(newSize >= _entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(_entries, 0, newEntries, 0, _count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (newEntries[i].HashCode != -1)
                    {
                        newEntries[i].HashCode = (_comparer.GetHashCode(newEntries[i].Key) & 0x7FFFFFFF);
                    }
                }
            }
            for (int i = 0; i < _count; i++)
            {
                if (newEntries[i].HashCode >= 0)
                {
                    int bucket = newEntries[i].HashCode % newSize;
                    newEntries[i].Next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            _buckets = newBuckets;
            _entries = newEntries;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (_buckets != null)
            {
                int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucket = hashCode % _buckets.Length;
                int last = -1;
                for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && _comparer.Equals(_entries[i].Key, key))
                    {
                        if (last < 0)
                        {
                            _buckets[bucket] = _entries[i].Next;
                        }
                        else
                        {
                            _entries[last].Next = _entries[i].Next;
                        }
                        _entries[i].HashCode = -1;
                        _entries[i].Next = _freeList;
                        _entries[i].Key = default(TKey);
                        _entries[i].Value = default(TValue);
                        _freeList = i;
                        _freeCount++;
                        _version++;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = _entries[i].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException();
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException();
            }

            KeyValuePair<TKey, TValue>[] pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[])
            {
                DictionaryEntry[] dictEntryArray = array as DictionaryEntry[];
                Entry[] entries = this._entries;
                for (int i = 0; i < _count; i++)
                {
                    if (entries[i].HashCode >= 0)
                    {
                        dictEntryArray[index++] = new DictionaryEntry(entries[i].Key, entries[i].Value);
                    }
                }
            }
            else
            {
                object[] objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException();
                }

                try
                {
                    int count = this._count;
                    Entry[] entries = this._entries;
                    for (int i = 0; i < count; i++)
                    {
                        if (entries[i].HashCode >= 0)
                        {
                            objects[index++] = new KeyValuePair<TKey, TValue>(entries[i].Key, entries[i].Value);
                        }
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    int i = FindEntry((TKey)key);
                    if (i >= 0)
                    {
                        return _entries[i].Value;
                    }
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (value == null && !(default(TValue) == null))
                    throw new ArgumentNullException("value");

                try
                {
                    TKey tempKey = (TKey)key;
                    try
                    {
                        this[tempKey] = (TValue)value;
                    }
                    catch (InvalidCastException)
                    {
                        throw new ArgumentException("value");
                    }
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("key");
                }
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return (key is TKey);
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (value == null && !(default(TValue) == null))
                throw new ArgumentNullException("value");

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add(tempKey, (TValue)value);
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("value");
                }
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("key");
            }
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
            IDictionaryEnumerator
        {
            private SerializableDictionary<TKey, TValue> dictionary;
            private int version;
            private int index;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(SerializableDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary._version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                if (version != dictionary._version)
                {
                    throw new InvalidOperationException();
                }

                // Use unsigned comparison since we set index to dictionary._count+1 when the enumeration ends.
                // dictionary._count+1 could be negative if dictionary._count is Int32.MaxValue
                while ((uint)index < (uint)dictionary._count)
                {
                    if (dictionary._entries[index].HashCode >= 0)
                    {
                        current = new KeyValuePair<TKey, TValue>(dictionary._entries[index].Key, dictionary._entries[index].Value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = dictionary._count + 1;
                current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new System.Collections.DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != dictionary._version)
                {
                    throw new InvalidOperationException();
                }

                index = 0;
                current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (index == 0 || (index == dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return current.Value;
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>, ICollection
        {
            private SerializableDictionary<TKey, TValue> dictionary;

            public KeyCollection(SerializableDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException("dictionary");
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                if (array.Length - index < dictionary.Count)
                {
                    throw new ArgumentException();
                }

                int count = dictionary._count;
                Entry[] entries = dictionary._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0) array[index++] = entries[i].Key;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return dictionary.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException();
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException();
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                if (array.Length - index < dictionary.Count)
                {
                    throw new ArgumentException();
                }

                TKey[] keys = array as TKey[];
                if (keys != null)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArgumentException();
                    }

                    int count = dictionary._count;
                    Entry[] entries = dictionary._entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].HashCode >= 0) objects[index++] = entries[i].Key;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException();
                    }
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            public struct Enumerator : IEnumerator<TKey>, IEnumerator
            {
                private SerializableDictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TKey currentKey;

                internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary._version;
                    index = 0;
                    currentKey = default(TKey);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (version != dictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    while ((uint)index < (uint)dictionary._count)
                    {
                        if (dictionary._entries[index].HashCode >= 0)
                        {
                            currentKey = dictionary._entries[index].Key;
                            index++;
                            return true;
                        }
                        index++;
                    }

                    index = dictionary._count + 1;
                    currentKey = default(TKey);
                    return false;
                }

                public TKey Current
                {
                    get
                    {
                        return currentKey;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (index == 0 || (index == dictionary._count + 1))
                        {
                            throw new InvalidOperationException();
                        }

                        return currentKey;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != dictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    index = 0;
                    currentKey = default(TKey);
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection : ICollection<TValue>, ICollection
        {
            private SerializableDictionary<TKey, TValue> dictionary;

            public ValueCollection(SerializableDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException("dictionary");
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                if (array.Length - index < dictionary.Count)
                {
                    throw new ArgumentException();
                }

                int count = dictionary._count;
                Entry[] entries = dictionary._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0) array[index++] = entries[i].Value;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return dictionary.ContainsValue(item);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException();
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException();
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                if (array.Length - index < dictionary.Count)
                    throw new ArgumentException();

                TValue[] values = array as TValue[];
                if (values != null)
                {
                    CopyTo(values, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArgumentException();
                    }

                    int count = dictionary._count;
                    Entry[] entries = dictionary._entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].HashCode >= 0) objects[index++] = entries[i].Value;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentException();
                    }
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private SerializableDictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TValue currentValue;

                internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary._version;
                    index = 0;
                    currentValue = default(TValue);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (version != dictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    while ((uint)index < (uint)dictionary._count)
                    {
                        if (dictionary._entries[index].HashCode >= 0)
                        {
                            currentValue = dictionary._entries[index].Value;
                            index++;
                            return true;
                        }
                        index++;
                    }
                    index = dictionary._count + 1;
                    currentValue = default(TValue);
                    return false;
                }

                public TValue Current
                {
                    get
                    {
                        return currentValue;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (index == 0 || (index == dictionary._count + 1))
                        {
                            throw new InvalidOperationException();
                        }

                        return currentValue;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != dictionary._version)
                    {
                        throw new InvalidOperationException();
                    }
                    index = 0;
                    currentValue = default(TValue);
                }
            }
        }
    }
}
