using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

namespace Celezt.DialogueSystem
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static partial class Tags
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

        private enum ElementType
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

        public static List<ITag> GetTagSequence(ReadOnlySpan<char> span, object? binder = null)
             => GetTagSequence(span, out _, binder);
        public static List<ITag> GetTagSequence(ReadOnlySpan<char> span, out int visibleCharacterCount, object? binder = null)
        {
            visibleCharacterCount = 0;
            using var openTagsObject = ListPool<ITagSpan>.Get(out var tagSpans);
            using var tagRangesObject = ListPool<(int Start, int End)>.Get(out var tagRanges);
            var tags = new List<ITag>();

            for (int index = 0; index < span.Length; index++)
            {
                if (span[index] is '<')
                {
                    if (!TryGetValidTagSpan(span.Slice(index), out var tagSpan, out var elementType))
                        goto NotValid;

                    if (!TryGetTagName(tagSpan, out var attributesSpan, out var tagName, out var tagVariant))
                        goto NotValid;

                    if (tagVariant is TagVariation.Invalid)
                        goto NotValid;

                    if (tagVariant is TagVariation.Unity)
                        goto Valid;

                    Type tagType = Types[tagName];
                    switch (elementType)
                    {
                        case ElementType.Start or ElementType.End when typeof(TagSingle).IsAssignableFrom(tagType):
                            throw new TagException($"{tagType} is a single tag and must use <.../> and not: {(elementType == ElementType.Start ? "<...>" : "</...>")}");
                        case ElementType.Marker when typeof(TagSpan).IsAssignableFrom(tagType):
                            throw new TagException($"{tagType} is a tag span, not a single tag. It should use <...> if start or </...> if end.");
                    }

                    if (elementType is ElementType.End)
                    {
                        int tagIndex = tagSpans.FindLastIndex(0, x => x.GetType() == tagType);

                        if (tagIndex == -1)    // No open tag of that type exist.
                            throw new TagException("Close tags must have an open tag of the same type.");

                        tagRanges[tagIndex] = (tagRanges[tagIndex].Start, visibleCharacterCount);
                        tagSpans.RemoveAt(tagIndex);    // Remove first last index of a tag.
                    }
                    else
                    {
                        using var attributesObject = ListPool<(string Name, string Value)?>.Get(out var attributes);
                        var tag = (ITag)Activator.CreateInstance(tagType);  // Create a new tag instance of specified type.

                        // Get all attributes.
                        if (attributesSpan[0] is '=')   // If it has implied attribute.
                        {
                            var implicitAttribute = GetAttribute(attributesSpan, out var nextSpan, isImplicit: true);
                            attributesSpan = nextSpan;
                            attributes.Add(implicitAttribute);
                        }

                        for (int i = 0; i < attributesSpan.Length; i++)
                        {
                            var attribute = GetAttribute(attributesSpan, out var nextSpan);
                            attributesSpan = nextSpan;

                            if (attribute is null)   // If there is no attributes left.
                                break;

                            attributes.Add(attribute);
                        }

                        foreach (var attr in attributes)
                            Bind(tag, attr?.Name!, attr?.Value!);

                        if (elementType is ElementType.Start)
                            tagSpans.Add((ITagSpan)tag);

                        tagRanges.Add((visibleCharacterCount, int.MinValue));
                        tags.Add(tag);
                    }

                    goto Valid;
                NotValid:
                    visibleCharacterCount += tagSpan.Length;
                Valid:
                    index += tagSpan.Length - 1;
                }
                else
                    visibleCharacterCount++;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                (int start, int end) = tagRanges[i];

                switch (tags[i])
                {
                    case ITagSingle tagSingle:  tagSingle.Initialize(start, binder);                            break;
                    case ITagSpan   tagSpan:    tagSpan.Initialize(new RangeInt(start, end - start), binder);   break;
                }
            }

            InvokeAll(tags);

            return tags;
        }

        public static string TrimTextTags(ReadOnlySpan<char> span, TagVariation excludeTagVariants = TagVariation.Custom | TagVariation.Unity)
        {
            Span<char> tempSpan = stackalloc char[span.Length];
            span.CopyTo(tempSpan);
            return TrimTextTags(tempSpan, excludeTagVariants).ToString();
        }

        public static Span<char> TrimTextTags(Span<char> span, TagVariation excludeTagVariants = TagVariation.Custom | TagVariation.Unity)
        {
            int count = 0;
            
            for (int index = 0; index < span.Length; index++)
            {
                if (span[index] is '<')
                {
                    if (!TryGetValidTagSpan(span.Slice(index), out var tagSpan, out _))
                        goto Copy;

                    if (!TryGetTagName(tagSpan, out _, out _, out TagVariation tagVariant))
                        goto Copy;

                    if (!excludeTagVariants.HasFlag(tagVariant))
                        goto Copy;

                    goto Ignore;
                Copy:
                    tagSpan.CopyTo(span.Slice(count));
                    count += tagSpan.Length;
                Ignore:
                    index += tagSpan.Length - 1;
                }
                else
                    span[count++] = span[index];
            }

            return span.Slice(0, count);
        }

        public static int GetTextLength(ReadOnlySpan<char> span, TagVariation excludeTagVariants = TagVariation.Custom | TagVariation.Unity)
        {
            int visibleCharacterCount = 0;

            for (int index = 0; index < span.Length; index++)
            {
                if (span[index] is '<')
                {                        
                    if (!TryGetValidTagSpan(span.Slice(index), out var tagSpan, out _))
                        goto NotValid;

                    if (!TryGetTagName(tagSpan, out _, out _, out var tagVariant))
                        goto NotValid;

                    if (!excludeTagVariants.HasFlag(tagVariant))
                        goto NotValid;

                    goto Valid;
                NotValid:
                    visibleCharacterCount += tagSpan.Length;
                Valid:
                    index += tagSpan.Length - 1;
                }
                else
                    visibleCharacterCount++;
            }
            
            return visibleCharacterCount;
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
    }

    public class TagException : Exception
    {
        public TagException(string message) : base(message) { }
    }
}
