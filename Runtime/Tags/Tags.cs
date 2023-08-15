using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

namespace Celezt.DialogueSystem
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class Tags
    {
        private static readonly HashSet<string> _unityRichTextTags = new(StringComparer.OrdinalIgnoreCase)
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

        public static IReadOnlyDictionary<Type, Type> SystemTypes
        {
            get
            {
                if (_systemTypes == null)
                    Initialize();

                return _systemTypes!;
            }
        }

        private static Dictionary<string, Type>? _types;
        private static Dictionary<Type, Type>? _systemTypes;
        private static Dictionary<Type, Dictionary<string, MemberInfo>> _cachedMembers = new();

        [Flags]
        public enum TagVariation
        {
            Invalid = 1 << 0,
            Custom = 1 << 1,
            Unity = 1 << 2,
        }

        private enum TagState
        {
            Start,
            End,
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
                    .Where(x => x.GetCustomAttribute<NonSerializedAttribute>() == null)
                    .ToDictionary(key => key.Name.ToKebabCase(), value => value);
            }
        }

        /// <summary>
        /// Invoke all tags and systems in order. Systems invokes after all of that type has been invoked.
        /// </summary>
        /// <param name="tags">To invoke.</param>
        public static void InvokeAll(IEnumerable<ITag> tags)
        {
            IOrderedEnumerable<ITag> sortedTypes = tags.OrderBy(x => x.GetType().GetCustomAttribute<CreateTagAttribute>().Order);
            Type? currentType = null;
            object? currentBinder = null;

            void InvokeSystem()
            {
                if (currentType is null)
                    return;

                if (SystemTypes.TryGetValue(currentType, out var systemType))
                {
                    object system = Activator.CreateInstance(systemType);
                    IList tagList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(currentType));

                    foreach (var tag in tags.Where(x => x.GetType() == currentType))
                        tagList.Add(tag);

                    systemType.GetMethod("OnCreate").Invoke(system, new[] { tagList, currentBinder });
                }
            }

            foreach (ITag tag in sortedTypes)
            {
                try
                {
                    if (currentType is not null && currentType != tag.GetType())    // Invoke system after all of that type is invoked.
                        InvokeSystem();

                    tag.OnCreate();

                    currentType = tag.GetType();
                    currentBinder = tag.Binder;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            InvokeSystem(); // Invoke on the last tag type.
        }

        public static List<ITag> GetTagSequence(string text, object? bind = null)
            => GetTagSequence(text, out _, bind);
        public static List<ITag> GetTagSequence(string text, out int visibleCharacterCount, object? binder = null)
        {
            visibleCharacterCount = 0;
            int beginIndex = 0;
            int endIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;
            var tagOpenList = new List<ITagSpan>();
            var tags = new List<ITag>();
            var tagRanges = new List<(int, int)>();

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                if (text[leftIndex] is '<' && (leftIndex <= 0 || text[leftIndex - 1] is not '\\'))
                {
                    beginIndex = leftIndex;

                    if (!IsTagValid(text, ref leftIndex, ref rightIndex, ref endIndex, out TagState state))
                    {
                        for (int i = beginIndex; i < endIndex; i++)
                        {
                            if (!IsBackslash(text, leftIndex))
                                visibleCharacterCount++;
                        }
                        continue;
                    }

                    TagVariation tagVariation = GetTagName(text, ref leftIndex, rightIndex - leftIndex + 1, out string tagName);
                    switch (tagVariation) // Ignore if not custom.
                    {
                        case TagVariation.Invalid:
                            for (int i = beginIndex; i <= endIndex; i++)
                            {
                                if (!IsBackslash(text, leftIndex))
                                    visibleCharacterCount++;
                            }
                            goto case TagVariation.Unity;    // Don't include as visible characters if unity tag.
                        case TagVariation.Unity:
                            leftIndex = endIndex;
                            continue;
                    }

                    (string? implicitName, string? implicitValue) = text[leftIndex++] is '=' ?  // If it has implied attributes.
                        GetAttribute(text, ref leftIndex, rightIndex - leftIndex + 1, isImplicit: true) : (null, null);

                    if (!string.IsNullOrEmpty(implicitName) && implicitName != "implicit")
                        throw new TagException("Implicit argument is not valid");

                    void BindAttributes(ITag tag)
                    {
                        if (!string.IsNullOrEmpty(implicitValue))
                            tag.Bind("implicit", implicitValue);

                        while (leftIndex <= rightIndex)
                        {
                            (string? name, string? value) = GetAttribute(text, ref leftIndex, rightIndex - leftIndex + 1);

                            if (name is null || value is null)   // If there is no attributes left.
                            {
                                leftIndex = endIndex;
                                break;
                            }

                            tag.Bind(name, value);
                        }
                    }

                    Type tagType = Types[tagName];
                    switch (state)
                    {
                        case TagState.Start or TagState.End when typeof(TagSingle).IsAssignableFrom(tagType):
                            throw new TagException($"{tagType} is a single tag and must use <.../> and not: {(state == TagState.Start ? "<...>" : "</...>")}");
                        case TagState.Marker when typeof(TagSpan).IsAssignableFrom(tagType):
                            throw new TagException($"{tagType} is a tag span, not a single tag. It should use <...> if start or </...> if end.");
                    }

                    switch (state)
                    {
                        case TagState.Start:
                            var tag = (TagSpan)Activator.CreateInstance(tagType);

                            BindAttributes(tag);

                            tagOpenList.Add(tag);
                            tagRanges.Add((visibleCharacterCount, int.MinValue));
                            tags.Add(tag);
                            break;
                        case TagState.End:
                            int tagIndex = tagOpenList.FindLastIndex(0, x => x.GetType() == tagType);

                            if (tagIndex == -1)    // No open tag of that type exist.
                                throw new TagException("Close tags must have an open tag of the same type.");

                            tagRanges[tagIndex] = (tagRanges[tagIndex].Item1, visibleCharacterCount);
                            tagOpenList.RemoveAt(tagIndex);    // Remove first last index of a tag.
                            break;
                        case TagState.Marker:
                            var tagMarker = (TagSingle)Activator.CreateInstance(tagType);

                            BindAttributes(tagMarker);

                            tagRanges.Add((visibleCharacterCount, int.MinValue));
                            tags.Add(tagMarker);
                            break;
                    }

                    leftIndex = endIndex;
                }
                else
                    visibleCharacterCount++;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                (int index, int closeIndex) = tagRanges[i];

                switch (tags[i])
                {
                    case ITagSingle tagSingle:
                        tagSingle.Initialize(index, binder);
                        break;
                    case ITagSpan tagSpan:
                        tagSpan.Initialize(new RangeInt(index, closeIndex - index), binder);
                        break;
                }
            }

            InvokeAll(tags);

            return tags;
        }

        public static string TrimTextTags(string text, TagVariation excludeTagType = TagVariation.Custom | TagVariation.Unity)
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

                    if (!IsTagValid(text, ref leftIndex, ref rightIndex, ref endIndex, out TagState state))
                    {
                        for (int i = beginIndex; i < endIndex; i++)
                        {
                            if (!IsBackslash(text, leftIndex))
                                span[newLength++] = text[i];
                        }
                        continue;
                    }

                    TagVariation tagType = GetTagName(text, ref leftIndex, rightIndex - leftIndex + 1, out _);
                    if (!excludeTagType.HasFlag(tagType))
                    {
                        for (int i = beginIndex; i <= endIndex; i++)
                        {
                            if (!IsBackslash(text, leftIndex))
                                span[newLength++] = text[i];
                        }
                    }

                    leftIndex = endIndex;
                }
                else if (!IsBackslash(text, leftIndex))
                    span[newLength++] = text[leftIndex];
            }

            return span.Slice(0, newLength).ToString();
        }

        public static int GetTextLength(string text, TagVariation excludeTagType = TagVariation.Custom | TagVariation.Unity)
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

                    if (!IsTagValid(text, ref leftIndex, ref rightIndex, ref endIndex, out _))
                    {
                        for (int i = beginIndex; i < endIndex; i++)
                        {
                            if (!IsBackslash(text, leftIndex))
                                visibleCharacterCount++;
                        }
                        continue;
                    }

                    TagVariation tagType = GetTagName(text, ref leftIndex, rightIndex - leftIndex + 1, out _);

                    if (!excludeTagType.HasFlag(tagType))
                    {
                        for (int i = beginIndex; i <= endIndex; i++)
                        {
                            if (!IsBackslash(text, leftIndex))
                                visibleCharacterCount++;
                        }
                    }

                    leftIndex = endIndex;
                }
                else if (!IsBackslash(text, leftIndex))
                    visibleCharacterCount++;
            }

            return visibleCharacterCount;
        }

        #region Private
        private static bool IsTagValid(string text, ref int leftIndex, ref int rightIndex, ref int endIndex, out TagState state)
        {
            char decoration = '\0';
            state = TagState.Start; // Start by default if it has no '/'.
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
                state = TagState.Marker;
                rightIndex--; // Before /.
            }
            else if (text[leftIndex] is '/')    // Tag is a closed tag. </tag>
            {
                state = TagState.End;
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

#if UNITY_EDITOR
        static Tags()
        {
            Initialize();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            _types = new(StringComparer.OrdinalIgnoreCase);
            _systemTypes = new();

            foreach (Type tagType in ReflectionUtility.GetTypesWithAttribute<CreateTagAttribute>(AppDomain.CurrentDomain))
            {
                if (tagType.GetInterface(nameof(ITag)) == null)
                    throw new TagException($"Object with '{nameof(CreateTagAttribute)}' are required to be derived from '{nameof(ITag)}'");

                string name = tagType.Name.TrimDecoration("Tag").ToKebabCase();
                _types[name] = tagType;
            }

            foreach (Type systemType in ReflectionUtility.GetTypesWithAttribute<CreateTagSystemAttribute>(AppDomain.CurrentDomain))
            {
                Type? foundInterfaceType = null;
                foreach (Type interfaceType in systemType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ITagSystem<,>))
                    {
                        foundInterfaceType = interfaceType;
                        break;
                    }   
                }

                if (foundInterfaceType == null)
                    throw new TagException($"Object with '{nameof(CreateTagSystemAttribute)}' are required to be derived from '{typeof(ITagSystem<,>).Name}'");

                _systemTypes[foundInterfaceType.GetGenericArguments()[0]] = systemType;
            }
        }
        #endregion
    }

    public class TagException : Exception
    {
        public TagException(string message) : base(message) { }
    }
}
