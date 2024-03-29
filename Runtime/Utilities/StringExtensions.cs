using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Celezt.DialogueSystem
{
    [DebuggerStepThrough]
    internal static class StringExtensions
    {
        public static string ToSnakeCase(this string text, bool trimUnderscore = true) 
            => ToSnakeCaseSpan(text, stackalloc char[text.Length * 2], trimUnderscore).ToString();
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
                if (currentChar is '_')
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

        public static string ToKebabCase(this string text, bool trimDash = true)
      => ToKebabCaseSpan(text, stackalloc char[text.Length * 2], trimDash).ToString();
        public static Span<char> ToKebabCaseSpan(this Span<char> span, bool trimDash = true)
        {
            Span<char> temp = stackalloc char[span.Length];
            span.CopyTo(temp);
            return span.Slice(0, ToKebabCaseSpan(temp, span, trimDash));
        }
        public static Span<char> ToKebabCaseSpan(this string text, Span<char> span, bool trimDash = true)
            => span.Slice(0, ToKebabCaseSpan(text.AsSpan(), span, trimDash));
        public static int ToKebabCaseSpan(this ReadOnlySpan<char> text, Span<char> span, bool trimDash = true)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return 0;

            UnicodeCategory? previousCategory = default;
            int length = 0;
            bool isTrimmable = true;

            for (int currentIndex = 0; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar is '-')
                {
                    if (trimDash && isTrimmable)  // skip all - until another letter is found.
                        continue;

                    span[length++] = '-';
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
                            span[length++] = '-';
                        }

                        currentChar = char.ToLower(currentChar, CultureInfo.InvariantCulture);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            span[length++] = '-';
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

        public static string ToTitleCase(this string text)
         => ToTitleCaseSpan(text, stackalloc char[text.Length * 2]).ToString();
        public static Span<char> ToTitleCaseSpan(this Span<char> span)
        {
            Span<char> temp = stackalloc char[span.Length];
            span.CopyTo(temp);
            return span.Slice(0, ToTitleCaseSpan(temp, span));
        }
        public static Span<char> ToTitleCaseSpan(this string text, Span<char> span)
            => span.Slice(0, ToTitleCaseSpan(text.AsSpan(), span));
        public static int ToTitleCaseSpan(this ReadOnlySpan<char> text, Span<char> span)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return 0;

            int length = 0;
            int currentIndex = 0;

            for (; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar is not '_' and not ' ')
                {
                    char nextChar = text[currentIndex + 1];
                    if (currentIndex + 1 < text.Length && char.IsUpper(currentChar) && char.IsUpper(nextChar)) // If current is upper, only the first will be upper.
                    {
                        span[length++] = currentChar;
                        span[length++] = char.ToLower(nextChar);
                        currentIndex += 2;
                        break;
                    }

                    span[length++] = char.ToUpper(currentChar);
                    currentIndex++;
                    break;
                }
            }

            for (; currentIndex < text.Length; currentIndex++)
            {
                char currentChar = text[currentIndex];
                if (currentChar is '_' or ' ' or '-')
                {
                    span[length++] = ' ';

                    if (currentIndex + 1 >= text.Length)
                        break;

                    char nextChar = text[currentIndex + 1];

                    if (nextChar is not '_' and not ' ' and not '-')
                    {
                        span[length++] = char.ToUpper(nextChar);
                        currentIndex++;
                    }

                    continue;
                }

                if (currentIndex + 1 < text.Length)
                {
                    char nextChar = text[currentIndex + 1];

                    if (char.IsLower(currentChar) && char.IsUpper(nextChar))    // If current is lower and next upper, make the next a new word. 
                    {
                        span[length++] = currentChar;
                        span[length++] = ' ';
                        span[length++] = nextChar;
                        currentIndex++;

                        continue;
                    }
                    else if (char.IsUpper(currentChar) && char.IsUpper(nextChar))    // If current is upper and next upper, make the next a new word.
                    {
                        span[length++] = currentChar;
                        span[length++] = ' ';
                        span[length++] = nextChar;
                        currentIndex++;

                        continue;
                    }
                }

                span[length++] = currentChar;
            }

            return length;
        }

        public static string ToPascalCase(this string text, bool trimUnderscore = true)
         => ToPascalCaseSpan(text, stackalloc char[text.Length], trimUnderscore).ToString();
        public static Span<char> ToPascalCaseSpan(this Span<char> span, bool trimUnderscore = true)
        {
            Span<char> temp = stackalloc char[span.Length];
            span.CopyTo(temp);
            return span.Slice(0, ToPascalCaseSpan(temp, span, trimUnderscore));
        }
        public static Span<char> ToPascalCaseSpan(this string text, Span<char> span, bool trimUnderscore = true)
            => span.Slice(0, ToPascalCaseSpan(text.AsSpan(), span, trimUnderscore));
        public static int ToPascalCaseSpan(this ReadOnlySpan<char> text, Span<char> span, bool trimUnderscore = true)
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
                    if (currentIndex + 1 < text.Length && char.IsUpper(currentChar) && char.IsUpper(nextChar)) // If current is upper, only the first will be upper.
                    {
                        span[length++] = currentChar;
                        span[length++] = char.ToLower(nextChar);
                        currentIndex += 2;
                        toLower = true;
                        break;
                    }

                    span[length++] = char.ToUpper(currentChar);
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
