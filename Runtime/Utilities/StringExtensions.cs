using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Unity.Profiling.Editor;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    internal static class StringExtensions
    {
        public static string ToSnakeCase(this string text, bool trimUnderscore = true) 
            => ToSnakeCaseSpan(text, stackalloc char[text.Length + Math.Min(2, text.Length / 5)], trimUnderscore).ToString();
        public static Span<char> ToSnakeCaseSpan(this Span<char> span, bool trimUnderscore = true)
        {
            Span<char> temp = stackalloc char[span.Length];
            span.CopyTo(temp);
            return span.Slice(0, ToSnakeCaseSpan(temp, span, trimUnderscore));
        }
        public static Span<char> ToSnakeCaseSpan(this string text, Span<char> span, bool trimUnderscore = true)
            => span.Slice(0, ToSnakeCaseSpan(text.AsSpan(), span, trimUnderscore));
        public static int ToSnakeCaseSpan(this ReadOnlySpan<char> text, Span<char> span, bool trimUnderscore = true)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return 0;

            UnicodeCategory? previousCategory = default;
            int length = 0;
            bool isTrimmable = true;

            for (int currentIndex = 0; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar == '_')
                {
                    if (trimUnderscore && isTrimmable)  // skip all _ until another letter is found.
                        continue;

                    span[length++] = '_';
                    previousCategory = null;
                    continue;
                }
                else
                    isTrimmable = false;

                var currentCategory = char.GetUnicodeCategory(currentChar);
                switch (currentCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                        if (previousCategory == UnicodeCategory.SpaceSeparator ||
                            previousCategory == UnicodeCategory.LowercaseLetter ||
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            previousCategory != null &&
                            currentIndex > 0 &&
                            currentIndex + 1 < text.Length &&
                            char.IsLower(text[currentIndex + 1]))
                        {
                            span[length++] = '_';
                        }

                        currentChar = char.ToLower(currentChar, CultureInfo.InvariantCulture);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            span[length++] = '_';
                        }
                        break;

                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }

                span[length++] = currentChar;
                previousCategory = currentCategory;
            }

            return length;
        }

        public static string ToCamelCase(this string text, bool trimUnderscore = true) 
            => ToCamelCaseSpan(text, stackalloc char[text.Length], trimUnderscore).ToString();
        public static Span<char> ToCamelCaseSpan(this Span<char> span, bool trimUnderscore = true)
        {
            Span<char> temp = stackalloc char[span.Length];
            span.CopyTo(temp);
            return span.Slice(0, ToCamelCaseSpan(temp, span, trimUnderscore));
        }
        public static Span<char> ToCamelCaseSpan(this string text, Span<char> span, bool trimUnderscore = true)
            => span.Slice(0, ToCamelCaseSpan(text.AsSpan(), span, trimUnderscore));
        public static int ToCamelCaseSpan(this ReadOnlySpan<char> text, Span<char> span, bool trimUnderscore = true)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return 0;

            int length = 0;
            int currentIndex = 0;
            bool toLower = false;

            for (; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar is not '_' and not ' ')
                {
                    if (!trimUnderscore && currentIndex >= 1)
                    {
                        if (text[currentIndex - 1] == '_')
                            span[length++] = '_';
                    }

                    char nextChar = text[currentIndex + 1];
                    if (currentIndex + 1 < text.Length && char.IsUpper(currentChar) && char.IsUpper(nextChar)) // If current is upper, both will be lower.
                    {
                        span[length++] = char.ToLower(currentChar);
                        span[length++] = char.ToLower(nextChar);
                        currentIndex += 2;
                        toLower = true;
                        break;
                    }

                    span[length++] = char.ToLower(currentChar);
                    currentIndex++;
                    break;
                }
            }

            for (; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar is '_' or ' ')
                {
                    toLower = false;

                    if (currentIndex + 1 >= text.Length)  
                        break;

                    char nextChar = text[currentIndex + 1];

                    if (nextChar is not '_' and not ' ')
                    {
                        span[length++] = char.ToUpper(nextChar);
                        currentIndex++;
                    }

                    continue;
                }

                if (currentIndex + 1 < text.Length)
                {
                    char nextChar = text[currentIndex + 1];

                    if (toLower)
                    {
                        if (char.IsUpper(currentChar) && char.IsLower(nextChar))    // Keep them the same if the next is lower but the current is upper.
                        {
                            toLower = false;
                            span[length++] = currentChar;
                            span[length++] = nextChar;
                            currentIndex++;

                            continue;
                        }
                    }
                    else
                    {
                        if (char.IsUpper(currentChar) && char.IsUpper(nextChar))    // If current is upper, next one is lower.
                        {
                            toLower = true;
                            span[length++] = currentChar;
                            span[length++] = char.ToLower(nextChar);
                            currentIndex++;

                            continue;
                        }
                    }
                }

                if (toLower)
                    span[length++] = char.ToLower(currentChar);
                else
                    span[length++] = currentChar;
            }

            return length;
        }

        public static string TrimDecoration(this string text, string decoration) 
            => TrimDecoration(text, decoration.AsSpan());
        public static string TrimDecoration(this string text, ReadOnlySpan<char> decoration)
            => TrimDecorationSpan(text, stackalloc char[text.Length], decoration).ToString();
        public static Span<char> TrimDecorationSpan(this Span<char> span, string decoration)
        {
            Span<char> temp = stackalloc char[span.Length];
            span.CopyTo(temp);
            return span.Slice(0, TrimDecorationSpan(temp, span, decoration));
        }
        public static Span<char> TrimDecorationSpan(this string text, Span<char> span, string decoration)
            => TrimDecorationSpan(text, span, decoration.AsSpan());
        public static Span<char> TrimDecorationSpan(this string text, Span<char> span, ReadOnlySpan<char> decoration)
            => span.Slice(0, TrimDecorationSpan(text.AsSpan(), span, decoration));
        public static int TrimDecorationSpan(this ReadOnlySpan<char> text, Span<char> span, ReadOnlySpan<char> decoration)
        {
            if (decoration.IsEmpty || decoration.IsWhiteSpace() || text.Length < decoration.Length)
            {
                text.CopyTo(span);
                return text.Length;
            }

            if (char.IsLower(decoration[0]))
                throw new ArgumentException("Decoration must start with an upper character.", nameof(decoration));

            for (int i = 0; i < text.Length; i++)
            {
                if (text.Length - i < decoration.Length)    // If there is not enough characters left.
                    break;

                if (text[i] == decoration[0])   // Check if decoration by checking if the first character matches. 
                {
                    // If letters exist after (Some'Decoration'{T}ype), then the next character must be uppercase.
                    if (text.Length > decoration.Length + i && !char.IsUpper(text[decoration.Length + i]))  // Invalid (Some'Decoration'{t}ype).
                        continue;

                    for (int j = 1; j < decoration.Length; j++)
                    {
                        if (text[i + j] != decoration[j])
                            break;
                        else if (j >= decoration.Length - 1)    // If last character in the decoration.
                        {
                            text.CopyTo(span);
                            span.Remove(i, decoration.Length);
                            return text.Length - decoration.Length;

                        }
                    }
                }
            }

            {
                text.CopyTo(span);
                return text.Length;
            }
        }
    }
}
