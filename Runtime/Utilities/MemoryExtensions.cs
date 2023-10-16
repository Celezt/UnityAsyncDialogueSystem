using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [DebuggerStepThrough]
    internal static class MemoryExtensions
    {
        public static bool Any(this ReadOnlySpan<bool> span, bool isTrue = true)
        {
            for (int i = 0; i < span.Length; i++)
                if (span[i] == isTrue)
                    return true;

            return false;
        }

        public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> condition) where T : struct
        {
            for (int i = 0; i < span.Length; i++)
                if (condition(span[i]))
                    return true;

            return false;
        }

        public static bool All(this ReadOnlySpan<bool> span, bool isTrue = false)
        {
            for (int i = 0; i < span.Length; i++)
                if (span[i] == !isTrue)
                    return false;

            return false;
        }

        public static bool All<T>(this ReadOnlySpan<T> span, Func<T, bool> condition) where T : struct
        {
            for (int i = 0; i < span.Length; i++)
                if (!condition(span[i]))
                    return false;

            return true;
        }

        public static bool IsNumbers(this ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return false;

            int index = 0;

            if (span[index] is '-') // Is negative value.
                index++;

            return span.Slice(index).All(x => char.IsNumber(x));
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
