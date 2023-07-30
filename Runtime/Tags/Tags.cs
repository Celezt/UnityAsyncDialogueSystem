using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace Celezt.DialogueSystem
{
    public static class Tags
    {
        private static readonly HashSet<string> _unityRichTextTags = new()
        {
            "a",
            "align",
            "allcaps",
            "alpha",
            "b",
            "br",
            "color",
            "cspace",
            "font",
            "font-weight",
            "gradient",
            "i",
            "indent",
            "line-height",
            "line-indent",
            "lowercase",
            "margin",
            "mark",
            "mspace",
            "nobr",
            "noparse",
            "pos",
            "rotate",
            "s",
            "size",
            "smallcaps",
            "space",
            "sprite",
            "strikethrough",
            "style",
            "sub",
            "sup",
            "u",
            "uppercase",
            "voffset",
            "width",
        };

        public static IReadOnlyDictionary<string, Type> Types
        {
            get
            {
                if (_types == null)
                    Initialize();

                return _types!;
            }
        }

        private static Dictionary<string, Type>? _types;
        private static Dictionary<Type, Dictionary<string, MemberInfo>> _cachedMembers = new();

        [Flags]
        public enum TagType
        {
            Invalid = 0,
            Custom = 1 << 0,
            Unity = 1 << 1,
        }

        private enum TagState
        {
            Open,
            Close,
            Marker
        }

        public static void Bind(this ITag tag, string name, string argument)
        {
            tag.GetMembers(out var members);

            MemberInfo member;
            if (name == "implicit")
            {
                member = members.Values.FirstOrDefault(x => x.GetCustomAttribute<ImplicitAttribute>() != null);

                if (member == null)
                    throw new TagException("No implicit property or field was found for " + tag.GetType().Name);
            }
            else if (!members.TryGetValue(name, out member))
                throw new TagException($"No '{name}' argument was found for " + tag.GetType().Name);

            member.SetValue(tag, Convert.ChangeType(argument, member.GetUnderlyingType()));
        }

        public static void GetMembers(this ITag tag, out Dictionary<string, MemberInfo> members)
            => GetMembers(tag.GetType(), out members);
        public static void GetMembers(Type type, out Dictionary<string, MemberInfo> members)
        {
            if (!_cachedMembers.TryGetValue(type.GetType(), out members))
            {
                _cachedMembers[type] = members = type  // Get all public field, properties and fields with SerializeFieldAttribute.
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                    .Cast<MemberInfo>()
                    .Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.IsPrivate ? x.GetCustomAttribute<SerializeField>() != null : x.IsPublic))
                    .ToDictionary(key => key.Name.ToKebabCase(), value => value);
            }
        }

        public static IEnumerable<ITag> GetSequence(string text)
            => GetSequence(text, out _);
        public static IEnumerable<ITag> GetSequence(string text, out int visibleCharacterCount)
        {
            visibleCharacterCount = 0;
            int beginIndex = 0;
            int endIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;
            var tagOpenList = new List<Tag>();
            var sequence = new List<ITag>();

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                if (text[leftIndex] is '<' && (leftIndex <= 0 || text[leftIndex - 1] is not '\\'))
                {
                    beginIndex = leftIndex;

                    if (!IsValidTag(text, ref leftIndex, ref rightIndex, ref endIndex, out TagState state))
                    {
                        leftIndex = rightIndex;
                        visibleCharacterCount += beginIndex - rightIndex + 1;
                        continue;
                    }

                    TagType tagType = ExtractTagName(text, ref leftIndex, rightIndex - leftIndex + 1, out string tagName);
                    switch (tagType) // Ignore if not custom.
                    {
                        case TagType.Invalid:
                            visibleCharacterCount += beginIndex - endIndex + 1;
                            goto case TagType.Unity;    // Don't include as visible characters if unity tag.
                        case TagType.Unity:
                            leftIndex = endIndex;
                            continue;
                    }

                    (string? name, string? argument) implicitArgument = text[leftIndex++] is '=' ?  // If it has implied arguments.
                        ExtractArgument(text, ref leftIndex, rightIndex - leftIndex + 1, isImplicit: true) : (null, null);

                    if (!string.IsNullOrEmpty(implicitArgument.name) && implicitArgument.name != "implicit")
                        throw new TagException("Implicit argument is not valid");

                    void BindArguments(ITag tag)
                    {
                        if (!string.IsNullOrEmpty(implicitArgument.argument))
                            tag.Bind("implicit", implicitArgument.argument);

                        while (leftIndex <= rightIndex)
                        {
                            (string? name, string? argument) = ExtractArgument(text, ref leftIndex, rightIndex - leftIndex + 1);

                            if (name is null || argument is null)   // If there is no arguments left.
                            {
                                leftIndex = endIndex;
                                break;
                            }

                            tag.Bind(name, argument);
                        }
                    }

                    switch (state)
                    {
                        case TagState.Open:
                            Tag tag = (Tag)Activator.CreateInstance(Types[tagName]);
                            tag._range = new RangeInt(visibleCharacterCount, -1);

                            BindArguments(tag);

                            tagOpenList.Add(tag);
                            tag.OnCreate();
                            sequence.Add(tag);
                            break;
                        case TagState.Close:
                            int tagIndex = tagOpenList.FindLastIndex(0, x => x.GetType() == Types[tagName]);

                            if (tagIndex == -1)    // No open tag of that type exist.
                                throw new TagException("Closure tags must have an open tag of the same type.");

                            var range = tagOpenList[tagIndex]._range;
                            range.length = visibleCharacterCount - range.start;
                            tagOpenList[tagIndex]._range = range;

                            tagOpenList.RemoveAt(tagIndex);    // Remove first last index of a tag.
                            break;
                        case TagState.Marker:
                            TagMarker tagMarker = (TagMarker)Activator.CreateInstance(Types[tagName]);
                            tagMarker._index = visibleCharacterCount;

                            BindArguments(tagMarker);

                            tagMarker.OnCreate();
                            sequence.Add(tagMarker);
                            break;
                    }
                }
                else
                    visibleCharacterCount++;
            }

            return sequence;
        }

        public static string TrimTextTags(string text, TagType excludeTagType = TagType.Custom | TagType.Unity)
        {
            Span<char> span = stackalloc char[text.Length];
            int newLength = 0;
            int beginIndex = 0;
            int endIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                if (text[leftIndex] is '<' && (leftIndex <= 0 || text[leftIndex - 1] is not '\\'))
                {
                    beginIndex = leftIndex;

                    if (!IsValidTag(text, ref leftIndex, ref rightIndex, ref endIndex, out TagState state))
                    {
                        for (leftIndex = beginIndex; leftIndex <= rightIndex; leftIndex++)
                            span[newLength++] = text[leftIndex];
                        continue;
                    }

                    TagType tagType = ExtractTagName(text, ref leftIndex, rightIndex - leftIndex + 1, out _);
                    if (!excludeTagType.HasFlag(tagType))
                    {
                        for (leftIndex = beginIndex; leftIndex <= endIndex; leftIndex++)
                            span[newLength++] = text[leftIndex];
                    }
                    else
                        leftIndex = endIndex;
                }
                else
                    span[newLength++] = text[leftIndex];
            }

            return span.Slice(0, newLength).ToString();
        }

        public static int GetTextLength(string text, TagType excludeTagType = TagType.Custom | TagType.Unity)
        {
            int visibleCharacterCount = 0;
            int beginIndex = 0;
            int endIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                if (text[leftIndex] is '<' && (leftIndex <= 0 || text[leftIndex - 1] is not '\\'))
                {
                    beginIndex = leftIndex;

                    if (!IsValidTag(text, ref leftIndex, ref rightIndex, ref endIndex, out TagState state))
                    {
                        visibleCharacterCount += beginIndex - rightIndex + 1;
                        continue;
                    }

                    TagType tagType = ExtractTagName(text, ref leftIndex, rightIndex - leftIndex + 1, out _);
                    if (!excludeTagType.HasFlag(tagType))
                        visibleCharacterCount += beginIndex - endIndex + 1;
                    else
                        leftIndex = endIndex;
                }
                else
                    visibleCharacterCount++;
            }

            return visibleCharacterCount;
        }

        #region Private
        private static bool IsValidTag(string text, ref int leftIndex, ref int rightIndex, ref int endIndex, out TagState state)
        {
            char decoration = '\0';
            state = TagState.Open; // Open by default if it has no '/'.

            if (!(text[leftIndex] is '<' && (leftIndex - 1 < 0 || text[leftIndex - 1] is not '\\')))
                return false;

            if (leftIndex + 1 >= text.Length)
                return false;

            for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
            {
                if (text[rightIndex] is '"' or '\'' && text[rightIndex - 1] is not '\\')
                {
                    if (text[rightIndex] == decoration)
                        decoration = '\0';
                    else
                        decoration = text[rightIndex];
                }

                if (decoration == '\0')
                {
                    if (text[rightIndex] is '<' && text[rightIndex - 1] is not '\\') // Invalid: must end with >. <tag> ! <tag<
                        return false;

                    if (text[rightIndex] is '>' && text[rightIndex - 1] is not '\\')
                        break;
                }

                if (rightIndex >= text.Length - 1)
                    return false;
            }

            endIndex = rightIndex;

            if (text[leftIndex + 1] is '/' && text[rightIndex - 1] is '/')   // Invalid: not allowed to have both. ! </tag/>
                return false;

            leftIndex++;    // After <.
            rightIndex--;   // Before >.

            if (text[leftIndex] is '/')    // Tag is an tag marker. </tag>
            {
                state = TagState.Marker;
                leftIndex++; // After /.
            }
            else if (text[rightIndex] is '/')    // Tag is an closure tag. <tag/>
            {
                state = TagState.Close;
                rightIndex--; // Before /.
            }

            return true;
        }

        private static void SkipWhitespace(string text, ref int index, int maxLength)
        {
            int startIndex = index;
            for (; index < startIndex + maxLength; index++)
                if (!char.IsWhiteSpace(text[index]))
                    return;
        }

        private static TagType ExtractTagName(string text, ref int index, int length, out string tagName)
        {
            int startIndex = index;
            int endIndex = index + length;
            tagName = string.Empty;

            for (; index < endIndex; index++) // <?=
            {
                if (text[index] is ' ' or '=')    // Ends if it finds a whitespace or =.
                    break;

                if (!char.IsLetter(text[index]) && text[index] != '-')    // Invalid: name must be a letter. <tag> ! <%3->
                    return TagType.Invalid;
            }

            tagName = text.Substring(startIndex, index - startIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

            if (_unityRichTextTags.Contains(tagName))   // If name is a unity tag.
                return TagType.Unity;

            if (Types.ContainsKey(tagName))   // If name is of custom tag.
                return TagType.Custom;

            return TagType.Invalid;    // If the tag does not exist.
        }

        private static (string? name, string? argument) ExtractArgument(string text, ref int index, int length, bool isImplicit = false)
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
                        throw new TagException("Argument names are not allowed to end with whitespace.");

                    if (!char.IsLetter(text[index]) && text[index] != '-')    // Invalid: name must be a letter. <tag> ! <%3->
                        throw new TagException($"Name cannot contain any numbers or symbols: '{text[index]}'");
                }
            }

            string name = isImplicit ? "implicit" : text.Substring(startIndex, index - startIndex);

            if (!isImplicit) // After =.
                index++;

            // 
            //  Extract Argument
            //
            char decoration = '\0';
            startIndex = index;
            if (text[index] is '"' or '\'') // Ignore decoration.
            {
                decoration = text[index++];

                for (; index < endIndex; index++)
                {
                    // Argument ending except when having a '\' in front of it.
                    if (text[index] == decoration && text[index - 1] is not '\\')
                        break;

                    if (index >= endIndex - 1)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                        throw new TagException("Arguments using \" or ' must close with the same character.");
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
                throw new TagException("Arguments cannot be empty");

            string argument = text.Substring(startIndex + (decoration is '\0' ? 0 : 1), index - startIndex - (decoration is '\0' ? 0 : 1));
            index += (decoration is '\0' ? 0 : 1);

            return (name, argument);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            _types = new();

            foreach (Type tagType in ReflectionUtility.GetTypesWithAttribute<CreateTagAttribute>(AppDomain.CurrentDomain))
            {
                if (tagType.GetInterface(nameof(ITag)) == null)
                    throw new TagException("Object with 'CreateTagAttribute' are required to be derived from 'ITag'");

                string name = tagType.Name.TrimDecoration("Tag").ToKebabCase();
                _types[name] = tagType;
            }
        }
        #endregion
    }

    public class TagException : Exception
    {
        public TagException(string message) : base(message) { }
    }
}
