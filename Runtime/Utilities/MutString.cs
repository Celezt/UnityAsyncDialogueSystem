using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Buffers;

namespace Celezt.DialogueSystem
{
    ///<summary>
    /// Mutable String class, optimized for speed and memory allocations while retrieving the final result as a string.
    /// Similarly to StringBuilder, but avoid a lot of allocations done by StringBuilder (conversion of int and float to string, frequent capacity change, etc.).
    ///</summary>
    ///<see cref="https://github.com/justinamiller/LiteStringBuilder"/>
    public class MutString
    {
        ///Working mutable string
        private char[] _buffer = null;
        private int _bufferPos = 0;
        private int _charsCapacity = 0;
        private const int DefaultCapacity = 16;
        private readonly static char[] _charNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly static CultureInfo _culture = CultureInfo.CurrentCulture;
        internal readonly static ArrayPool<char> Pool_Instance = ArrayPool<char>.Shared;

#pragma warning disable HAA0501 // Explicit new array type allocation
        private readonly static char[][] s_bool = new char[2][]
#pragma warning restore HAA0501 // Explicit new array type allocation
  {
            new char[]{ 'F','a','l','s','e'},
            new char[]{ 'T', 'r','u','e' }
  };

        public int Length
        {
            get => _bufferPos;
            set => _bufferPos = value;
        }

        public char this[int index]
        {
            get
            {
                if (index > _bufferPos || 0 > index)
                    throw new IndexOutOfRangeException();

                return _buffer[index];
            }
            set
            {
                if (index > _bufferPos || 0 > index)
                    throw new ArgumentOutOfRangeException("index");

                _buffer[index] = value;
            }
        }

        /// <summary>
        /// Get a new instance of <see cref="MutString"/>
        /// </summary>
        public static MutString Create(int initialCapacity = DefaultCapacity) => new MutString(initialCapacity);

        public MutString(int initialCapacity = DefaultCapacity)
        {
            int capacity = initialCapacity > 0 ? initialCapacity : DefaultCapacity;
            _buffer = Pool_Instance.Rent(capacity);
            _charsCapacity = _buffer.Length;
        }

        public MutString(string value)
        {
            if (value != null)
            {
                int capacity = value.Length > 0 ? value.Length : DefaultCapacity;
                _buffer = Pool_Instance.Rent(capacity);
                _charsCapacity = _buffer.Length;
                this.Append(value);
            }
            else
            {
                _buffer = Pool_Instance.Rent(DefaultCapacity);
                _charsCapacity = _buffer.Length;
            }
        }

        public bool IsEmpty() => _bufferPos == 0;

        ///<summary>
        /// Allocates a string.
        ///</summary>
        public override string ToString()
        {
            if (_bufferPos == 0)
                return string.Empty;

            unsafe
            {
                fixed (char* sourcePtr = &_buffer[0])
                    return new string(sourcePtr, 0, _bufferPos);
            }
        }

        public override bool Equals(object obj) => Equals(obj as MutString);
        public bool Equals(MutString other)
        {
            // Check for null.
            if (other is null)
                return false;

            // Check for same reference.
            if (ReferenceEquals(this, other))
                return true;

            // Check for same Id and same Values.
            if (other.Length != this.Length)
                return false;

            for (var i = 0; i < _bufferPos; i++)
            {
                if (!this._buffer[i].Equals(other._buffer[i]))
                    return false;
            }

            return true;
        }

        ///<summary>
        /// Sets a string without memory allocation.
        ///</summary>
        public void Set(string str)
        {
            // Fills the _chars list to manage future appends, but we also directly set the final stringGenerated.
            Clear();
            Append(str);
        }

        ///<summary>
        /// Clears values, and append new values. Will allocate a little memory due to boxing.
        ///</summary>
        public void Set(params object[] values)
        {
            Clear();

            for (int i = 0; i < values.Length; i++)
                Append(values[i]);
        }

        ///<summary>
        /// Sets buffer pointer to zero.
        ///</summary>
        public MutString Clear()
        {
            _bufferPos = 0;
            return this;
        }

        ///<summary>
        /// Appends a new line.
        ///</summary>
        public MutString AppendLine() => Append(Environment.NewLine);
        ///<summary>
        /// Appends a string and new line without memory allocation.
        ///</summary>
        public MutString AppendLine(string value) => Append(value).Append(Environment.NewLine);

        ///<summary>
        /// Allocates on the array's creation, and on boxing values.
        ///</summary>
        public MutString Append(params object[] values)
        {
            if (values != null)
            {
                int len = values.Length;
                for (var i = 0; i < len; i++)
                    this.Append<object>(values[i]);
            }

            return this;
        }
        ///<summary>
        /// Allocates on the array's creation.
        ///</summary>
        public MutString Append<T>(params T[] values)
        {
            if (values != null)
            {
                int len = values.Length;
                for (var i = 0; i < len; i++)
                    Append(values[i]);
            }

            return this;
        }
        private void Append<T>(T value)
        {
            if (value == null)
                return;

            switch (value)
            {
                case string:    Append(value as string);        break;
                case char:      Append((char)(object)value);    break;
                case char[]:    Append((char[])(object)value);  break;
                case int:       Append((int)(object)value);     break;
                case long:      Append((long)(object)value);    break;
                case bool:      Append((bool)(object)value);    break;
                case DateTime:  Append((DateTime)(object)value);break;
                case decimal:   Append((decimal)(object)value); break;
                case float:     Append((float)(object)value);   break;
                case double:    Append((double)(object)value);  break;
                case byte:      Append((byte)(object)value);    break;
                case sbyte:     Append((sbyte)(object)value);   break;
                case ulong:     Append((ulong)(object)value);   break;
                case uint:      Append((uint)(object)value);    break;
                default:        Append(value.ToString());       break;
            }
        }
        ///<summary>
        /// Appends a <see cref="string"/> without memory allocation.
        ///</summary>
        public MutString Append(string value)
        {
            int n = value?.Length ?? 0;
            if (n > 0)
            {
                EnsureCapacity(n);

                value.AsSpan().TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
                _bufferPos += n;
            }

            return this;
        }
        ///<summary> 
        /// Appends a <see cref="char"/> without memory allocation.
        ///</summary>
        public MutString Append(char value)
        {
            if (_bufferPos >= _charsCapacity)
                EnsureCapacity(1);

            _buffer[_bufferPos++] = value;
            return this;
        }
        ///<summary>
        /// Appends a <see cref="bool"/> without memory allocation.
        ///</summary>
        public MutString Append(bool value) => value ? Append(s_bool[1]) : Append(s_bool[0]);
        ///<summary>
        /// Appends a <see cref="char"/>[] without memory allocation.
        ///</summary>
        public MutString Append(char[] value)
        {
            if (value != null)
            {
                int n = value.Length;
                if (n > 0)
                {
                    EnsureCapacity(n);
                    new Span<char>(value).TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
                    _bufferPos += n;
                }
            }

            return this;
        }
        ///<summary>
        /// Appends an <see cref="object.ToString()"/>. Allocates memory.
        ///</summary>
        public MutString Append(object value)
        {
            if (value is null)
                return this;

            return Append(value.ToString());
        }
        ///<summary>
        /// Appends an <see cref="DateTime"/>. Allocates memory.
        ///</summary>
        public MutString Append(DateTime value) => Append(value.ToString(_culture));
        ///<summary>
        /// Appends an <see cref="sbyte"/> without memory allocation.
        ///</summary>
        public MutString Append(sbyte value)
        {
            if (value < 0)
                return Append((ulong)-((int)value), true);

            return Append((ulong)value, false);
        }
        ///<summary>
        /// Appends an <see cref="byte"/> without memory allocation.
        ///</summary>
        public MutString Append(byte value) => Append(value, false);
        ///<summary>
        /// Appends an <see cref="uint"/> without memory allocation.
        ///</summary>
        public MutString Append(uint value) => Append((ulong)value, false);
        /// <summary>
        /// Appends a <see cref="ulong"/> without memory allocation.
        ///</summary>
        public MutString Append(ulong value) => Append(value, false);
        ///<summary>
        /// Appends an <see cref="short"/> without memory allocation.
        ///</summary>
        public MutString Append(short value) => Append((int)value);
        ///<summary>
        /// Appends an <see cref="int"/> without memory allocation.
        ///</summary>
        public MutString Append(int value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }

            return Append((ulong)value, isNegative);
        }
        ///<summary>
        /// Appends an <see cref="long"/> without memory allocation.
        ///</summary>
        public MutString Append(long value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }

            return Append((ulong)value, isNegative);
        }
        ///<summary>
        /// Appends a <see cref="float"/>. Allocates memory.
        ///</summary>
        public MutString Append(float value) => Append(value.ToString(_culture));
        ///<summary>
        /// Appends a <see cref="decimal"/>. Allocates memory.
        ///</summary>
        public MutString Append(decimal value) => Append(value.ToString(_culture));
        ///<summary>
        /// Appends a <see cref="double"/>. Allocates memory.
        ///</summary>
        public MutString Append(double value) => Append(value.ToString(_culture));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MutString Append(ulong value, bool isNegative)
        {
            // Allocate enough memory to handle any ulong number.
            int length = GetIntLength(value);


            EnsureCapacity(length + (isNegative ? 1 : 0));
            var buffer = _buffer;

            // Handle the negative case.
            if (isNegative)
            {
                buffer[_bufferPos++] = '-';
            }
            if (value <= 9)
            {
                //between 0-9.
                buffer[_bufferPos++] = _charNumbers[value];
                return this;
            }

            // Copy the digits with reverse in mind.
            _bufferPos += length;
            int nbChars = _bufferPos - 1;
            do
            {
                buffer[nbChars--] = _charNumbers[value % 10];
                value /= 10;
            } while (value != 0);

            return this;
        }

        ///<summary>
        /// Replaces all occurrences of a <see cref="string"/> by another one.
        ///</summary>
        public MutString Replace(string oldStr, string newStr)
        {
            if (_bufferPos == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;

            int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
            int size = ((_bufferPos / oldstrLength) * (oldstrLength + deltaLength)) + 1;
            int index = 0;
            char[] replacementChars = null;
            int replaceIndex = 0;
            char firstChar = oldStr[0];

            // Create the new string into _replacement.
            for (int i = 0; i < _bufferPos; i++)
            {
                bool isToReplace = false;
                if (_buffer[i] == firstChar) // If first character found, check for the rest of the string to replace.
                {
                    int k = 1; // Skip one char.
                    while (k < oldstrLength && _buffer[i + k] == oldStr[k])
                        k++;

                    isToReplace = (k == oldstrLength);
                }
                if (isToReplace) // Do the replacement.
                {
                    if (replaceIndex == 0)
                    {
                        // First replacement target.
                        replacementChars = Pool_Instance.Rent(size);
                        // Copy first set of char that did not match.
                        new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                        index = i;
                    }

                    replaceIndex++;
                    i += oldstrLength - 1;

                    for (int k = 0; k < newStrLength; k++)
                        replacementChars[index++] = newStr[k];
                }
                else if (replaceIndex > 0) // No replacement, copy the old character.
                    replacementChars[index++] = _buffer[i]; // Todo: Could batch these up instead one at a time!
            }

            if (replaceIndex > 0)
            {
                // Copy back the new string into _chars.
                EnsureCapacity(index - _bufferPos);

                new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));

                Pool_Instance.Return(replacementChars);
                _bufferPos = index;
            }

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int appendLength)
        {
            int capacity = _charsCapacity;
            int pos = _bufferPos;
            if (pos + appendLength > capacity)
            {
                capacity = capacity + appendLength + DefaultCapacity - (capacity - pos);
                char[] newBuffer = Pool_Instance.Rent(capacity);

                if (pos > 0)
                    new Span<char>(_buffer, 0, _bufferPos).TryCopyTo(new Span<char>(newBuffer)); // Copy data.

                Pool_Instance.Return(_buffer);

                _buffer = newBuffer;
                _charsCapacity = newBuffer.Length;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;
                for (var i = 0; i < _bufferPos; i++)
                    hash += _buffer[i].GetHashCode();

                return 31 * hash + _bufferPos;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIntLength(ulong n)
        {
            if (n < 10L) return 1;
            if (n < 100L) return 2;
            if (n < 1000L) return 3;
            if (n < 10000L) return 4;
            if (n < 100000L) return 5;
            if (n < 1000000L) return 6;
            if (n < 10000000L) return 7;
            if (n < 100000000L) return 8;
            if (n < 1000000000L) return 9;
            if (n < 10000000000L) return 10;
            if (n < 100000000000L) return 11;
            if (n < 1000000000000L) return 12;
            if (n < 10000000000000L) return 13;
            if (n < 100000000000000L) return 14;
            if (n < 1000000000000000L) return 15;
            if (n < 10000000000000000L) return 16;
            if (n < 100000000000000000L) return 17;
            if (n < 1000000000000000000L) return 18;
            if (n < 10000000000000000000L) return 19;

            return 20;
        }
    }
}
