using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public static partial class Tags
    {
        private static bool TryGetValidTagSpan(ReadOnlySpan<char> span, out ReadOnlySpan<char> tagSpan, out ElementType block)
        {
            tagSpan = span;
            block = ElementType.Start; // Start by default if it has no '/'.
            int index = 0;

            if (span.Length < 3)
                return false;

            for (; index < span.Length; index++)
                if (span[index] is '<')
                    break;

            if (index == span.Length)
                return false;

            span = span.Slice(index);

            if (span[++index] is '>') // Invalid: must contain a name of at least one letter. <b> ! <>, <<
                goto Invalid;

            char decoration = '\0';

            for (; index < span.Length; index++)
            {
                char chr = span[index];

                if (chr is '"' or '\'')
                {
                    if (decoration is '\0') // Assign as new decoration if none currently exist.
                        decoration = chr;
                    else if (chr == decoration) // Found the ending of the string.
                        decoration = '\0';
                }
                else if (decoration is '\0')
                {
                    if (chr is '<') // Invalid: must end with >. <tag> ! <tag<
                    {
                        index--;    // Start at <.
                        goto Invalid;
                    }

                    if (chr is '>') // Found the ending.
                        break;
                }

                if (index + 1 == span.Length)   // Could not find the ending to the tag inside the span.
                    goto Invalid;
            }

            char leftNextChar = span[1];
            char rightPreviousChar = span[index - 1];

            tagSpan = span.Slice(0, index + 1);  // <...>

            if (leftNextChar is '/' && rightPreviousChar is '/')   // Invalid: not allowed to have both. ! </tag/>
                return false;

            if (leftNextChar is '/')            // Tag is a closed tag. </tag>
                block = ElementType.End;
            else if (rightPreviousChar is '/')  // Tag is a single tag. <tag/>
                block = ElementType.Marker;

            return true;

            Invalid:
                tagSpan = span.Slice(0, index + 1);
                return false;
        }

        private static bool IsTagValid(ReadOnlySpan<char> text, ref int leftIndex, ref int rightIndex, ref int endIndex, out ElementType elementType)
        {
            char decoration = '\0';
            elementType = ElementType.Start; // Start by default if it has no '/'.
            endIndex = rightIndex = leftIndex + 1;

            if (!(text[leftIndex] is '<' && (leftIndex - 1 < 0 || text[leftIndex - 1] is not '\\')))
                return false;

            if (leftIndex + 1 >= text.Length)
                return false;

            if (text[leftIndex + 1] is '>') // Invalid: must contain a name. <tag> ! <>
                return false;

            for (; rightIndex < text.Length; rightIndex++)
            {
                endIndex = rightIndex;

                if (text[rightIndex] is '"' or '\'' && text[rightIndex - 1] is not '\\')
                {
                    if (text[rightIndex] == decoration)
                        decoration = '\0';
                    else
                        decoration = text[rightIndex];
                }

                if (decoration is '\0')
                {
                    if (text[rightIndex] is '<' && text[rightIndex - 1] is not '\\') // Invalid: must end with >. <tag> ! <tag<
                    {
                        leftIndex = --rightIndex;
                        return false;
                    }

                    if (text[rightIndex] is '>' && text[rightIndex - 1] is not '\\')
                        break;
                }

                if (rightIndex + 1 >= text.Length)
                {
                    leftIndex = --rightIndex;
                    return false;
                }
            }

            if (text[leftIndex + 1] is '/' && text[rightIndex - 1] is '/')   // Invalid: not allowed to have both. ! </tag/>
                return false;

            leftIndex++;    // After <.
            rightIndex--;   // Before >.

            if (text[rightIndex] is '/')    // Tag is a single tag. <tag/>
            {
                elementType = ElementType.Marker;
                rightIndex--; // Before /.
            }
            else if (text[leftIndex] is '/')    // Tag is a closed tag. </tag>
            {
                elementType = ElementType.End;
                leftIndex++; // After /.
            }

            return true;
        }

        private static bool IsBackslash(string text, int index) => text[index] is '\\' && (index <= 0 || text[index - 1] is not '\\');

        private static void SkipWhitespace(string text, ref int index, int maxLength)
        {
            int startIndex = index;
            for (; index < startIndex + maxLength; index++)
                if (!char.IsWhiteSpace(text[index]))
                    return;
        }

        private static bool TryGetTagName(ReadOnlySpan<char> span, out ReadOnlySpan<char> attributesSpan, out string tagName, out TagVariation tagVariant)
        {
            attributesSpan = ReadOnlySpan<char>.Empty;
            tagVariant = TagVariation.Invalid;  // Invalid by default.
            tagName = string.Empty;
            int index = 0;

            // Skip first characters if they exist.
            if (span[index] is '<')
                index++;
            if (span[index] is '/')
                index++;

            int startIndex = index;
            for (; index < span.Length; index++) // <?=
            {
                char chr = span[index];

                if (chr is ' ' or '=' or '>')    // Ends if it finds a whitespace or =.
                    break;

                if (!char.IsLetter(chr) && chr is not '-')    // Invalid: name must be a letter. <tag-name> ! <%3+>
                    return false;
            }

            tagName = span.Slice(startIndex, index - startIndex).ToString();  // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

            if (_unityRichTextTags.Contains(tagName))   // If name is a unity tag.
                tagVariant = TagVariation.Unity;
            else if (Types.ContainsKey(tagName))        // If name is of custom tag.
                tagVariant = TagVariation.Custom;

            int length = span.Length - index - 1;   // Ignore >.
            if (span[index + length - 1] is '/')        // Ignore /.
                length--;

            attributesSpan = span.Slice(index, length);

            return true;
        }

        private static TagVariation GetTagName(string text, ref int index, int length, out string tagName)
        {
            int startIndex = index;
            int endIndex = index + length;
            tagName = string.Empty;

            for (; index < endIndex; index++) // <?=
            {
                if (text[index] is ' ' or '=')    // Ends if it finds a whitespace or =.
                    break;

                if (!char.IsLetter(text[index]) && text[index] is not '-' and not '\\')    // Invalid: name must be a letter. <tag-name> ! <%3+>
                    return TagVariation.Invalid;
            }

            tagName = text.Substring(startIndex, index - startIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

            if (_unityRichTextTags.Contains(tagName))   // If name is a unity tag.
                return TagVariation.Unity;

            if (Types.ContainsKey(tagName))   // If name is of custom tag.
                return TagVariation.Custom;

            return TagVariation.Invalid;    // If the tag does not exist.
        }

        private static (string Name, string Value)? GetAttribute(ReadOnlySpan<char> span, out ReadOnlySpan<char> nextSpan, bool isImplicit = false)
        {
            nextSpan = ReadOnlySpan<char>.Empty;
            char chr = '\0';
            int index = 0;

            for (; index < span.Length; index++)
                if (!char.IsWhiteSpace(span[index]))
                    break;

            if (index == span.Length)
                return null;

            //
            //  Extract Name
            //
            int nameIndex = index;
            if (!isImplicit)    // Skip if implicit.
            {
                for (; index < span.Length; index++) // <?=
                {
                    chr = span[index];

                    if (chr is '=')    // Ends if it finds a =.
                        break;

                    if (chr is ' ')
                        throw new TagException("Attribute name are not allowed to end with whitespace.");

                    if (!char.IsLetter(chr) && chr != '-')    // Invalid: name must be a letter. <tag> ! <%3->
                        throw new TagException($"Name cannot contain any numbers or symbols: '{chr}'.");
                }
            }

            string name = isImplicit ? "implicit" : span.Slice(nameIndex, index - nameIndex).ToString();

            // 
            //  Extract Attribute
            //
            char decoration = '\0';
            int attributeIndex = ++index;
            chr = span[attributeIndex];
            if (chr is '"' or '\'') // Ignore decoration.
            {
                decoration = span[index++];

                for (; index < span.Length; index++)
                {
                    chr = span[index];

                    if (chr == decoration) // Attribute ending.
                        break;

                    if (index + 1 == span.Length)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                        throw new TagException("Attributes using \" or ' must close with the same character.");
                }
            }
            else if (chr is not ' ')    // If no decorations are present. WARNING: whitespace means end!
            {
                for (; index < span.Length; index++)
                {
                    chr = span[index];

                    if (char.IsWhiteSpace(chr))
                        break;

                    // Invalid: not allowed to use these characters when using no decorations.
                    if (chr is '/' or '"' or '\'')
                        throw new TagException("Not allowed to use /, \" or ' when using no decoration.");
                }
            }
            else
                throw new TagException("Attribute value cannot be empty.");

            int decorationOffset = decoration is '\0' ? 0 : 1;
            string value = span.Slice(attributeIndex + decorationOffset, index - attributeIndex - decorationOffset).ToString();
            nextSpan = span.Slice(index);

            return (name, value);
        }

        private static (string? name, string? value) GetAttribute(string text, ref int index, int length, bool isImplicit = false)
        {
            int endIndex = index + length;

            SkipWhitespace(text, ref index, length);

            if (index == endIndex)
                return (null, null);

            //
            //  Extract Name
            //
            int startIndex = index;
            if (!isImplicit)
            {
                for (; index < endIndex; index++) // <?=
                {
                    if (text[index] is '=')    // Ends if it finds a =.
                        break;

                    if (text[index] is ' ')
                        throw new TagException("Attribute name are not allowed to end with whitespace.");

                    if (!char.IsLetter(text[index]) && text[index] != '-')    // Invalid: name must be a letter. <tag> ! <%3->
                        throw new TagException($"Name cannot contain any numbers or symbols: '{text[index]}'");
                }
            }

            string name = isImplicit ? "implicit" : text.Substring(startIndex, index - startIndex);

            if (!isImplicit) // After =.
                index++;

            // 
            //  Extract Attribute
            //
            char decoration = '\0';
            startIndex = index;
            if (text[index] is '"' or '\'') // Ignore decoration.
            {
                decoration = text[index++];

                for (; index < endIndex; index++)
                {
                    // Attribute ending except when having a '\' in front of it.
                    if (text[index] == decoration && text[index - 1] is not '\\')
                        break;

                    if (index >= endIndex - 1)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                        throw new TagException("Attributes using \" or ' must close with the same character.");
                }
            }
            else if (text[index] is not ' ')    // If no decorations are present. WARNING: whitespace means end!
            {
                for (; index < endIndex; index++)
                {
                    if (char.IsWhiteSpace(text[index]))
                        break;

                    // Invalid: not allowed to use these characters except when having a '\' in front of it.
                    if (text[index] is '/' or '"' or '\'' && text[index - 1] is not '\\')
                        throw new TagException("Not allowed to use /, \" or ' except when having a \\ in front of it.");
                }
            }
            else
                throw new TagException("Attribute value cannot be empty");

            string value = text.Substring(startIndex + (decoration is '\0' ? 0 : 1), index - startIndex - (decoration is '\0' ? 0 : 1));
            index += (decoration is '\0' ? 0 : 1);

            return (name, value);
        }
    }
}
