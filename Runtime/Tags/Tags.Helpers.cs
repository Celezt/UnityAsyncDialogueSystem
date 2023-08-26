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
    }
}
