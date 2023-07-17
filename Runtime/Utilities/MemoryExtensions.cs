using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    internal static class MemoryExtensions
    {
        public static bool Any(this Span<bool> span)
        {
            for (int i = 0; i < span.Length; i++)
                if (span[i] == true)
                    return true;

            return false;
        }

        public static Span<char> Remove(this Span<char> span, int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentException(nameof(startIndex), "StartIndex cannot be negative.");

            if (span.Length < length || length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be longer than the span nor negative.");

            for (; startIndex < span.Length - length; startIndex++)
                span[startIndex] = span[startIndex + length];

            return span.Slice(0, span.Length - length);
        }
    }
}
