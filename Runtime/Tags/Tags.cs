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
                    .ToDictionary(key => key.Name.ToCamelCase(), value => value);
            }
        }

        public static IEnumerable<ITag> GetSequence(string text)
        {
            int beginIndex = 0;
            int endIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;
            int newLength = 0;
            var state = TagState.Open;
            var tagOpenList = new List<Tag>();

            bool HasNext() => leftIndex + 1 < text.Length;
            bool HasPrevious() => leftIndex - 1 >= 0;

            bool TagName(out string name)
            {
                name = string.Empty;
                int beforeIndex = leftIndex;

                for (; leftIndex <= rightIndex; leftIndex++) // <?=
                {
                    if (text[leftIndex] is ' ' or '=')    // Ends if it finds a whitespace or =.
                        break;

                    if (!char.IsLetter(text[leftIndex]))    // Invalid: name must be a letter. <tag> ! <%3->
                        throw new TagException("Name cannot contain any numbers or symbols");
                }

                string slice = text.Substring(beforeIndex, leftIndex - beforeIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

                if (!Types.TryGetValue(slice, out Type type))   // If the tag does not exist.
                    return false;

                if ((typeof(Tag).IsAssignableFrom(type) && state is not TagState.Open and not TagState.Close) ||
                    (typeof(TagMarker).IsAssignableFrom(type) && state is not TagState.Marker))
                    throw new TagException($"Tag type: {type} is not derived from Tag");

                name = slice;

                return true;    // Name is valid.
            }

            bool Argument(out string? argument)
            {
                argument = null;

                if (state == TagState.Close)    // Invalid: no arguments allowed on a closing tag. <tag/> ! <tag=?/>
                    throw new TagException("Closure tags are not allowed to contain any arguments."); 

                int beforeIndex = leftIndex;
                char decoration = '\0';

                if (text[leftIndex] is '"' or '\'') // Ignore decoration.
                {
                    beforeIndex++;
                    decoration = text[leftIndex++];

                    for (; leftIndex <= rightIndex; leftIndex++)
                    {
                        // Argument ending except when having a '\' in front of it.
                        if (text[leftIndex] == decoration && text[leftIndex - 1] is not '\\')
                            break;

                        if (leftIndex >= rightIndex)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                            throw new TagException("Arguments using \" or ' must close with the same character.");
                    }
                }
                else if (text[leftIndex] is not ' ')    // If no decorations are present. WARNING: whitespace means end!
                {
                    for (; leftIndex <= rightIndex; leftIndex++)
                    {
                        if (char.IsWhiteSpace(text[leftIndex]))
                            break;

                        // Invalid: not allowed to use these characters except when having a '\' in front of it.
                        if (text[leftIndex] is '/' or '"' or '\'' && text[leftIndex - 1] is not '\\')
                            throw new TagException("Not allowed to use /, \" or ' except when having a \\ in front of it.");
                    }
                }
                else
                    throw new TagException("Arguments cannot be empty");

                argument = text.Substring(beforeIndex, leftIndex - beforeIndex);
                leftIndex += (decoration is '\0' ? 0: 1);

                return true;
            }

            IEnumerable<(string Name, string Argument)> Arguments()
            {
                string ArgumentName()
                {
                    int beforeIndex = leftIndex;
                    for (; leftIndex < rightIndex; leftIndex++) // <?=
                    {
                        if (text[leftIndex] is '=')    // Ends if it finds a =.
                            break;

                        if (text[leftIndex] is ' ')
                            throw new TagException("Argument names are not allowed to end with whitespace.");

                        if (!char.IsLetter(text[leftIndex]))    // Invalid: name must be a letter. <tag> ! <%3->
                            throw new TagException("Name cannot contain any numbers or symbols");
                    }

                    string slice = text.Substring(beforeIndex, leftIndex - beforeIndex);

                    return slice;
                }

                while(leftIndex <= rightIndex)
                {
                    if (char.IsWhiteSpace(text[leftIndex])) // Skip all white spaces.
                    {
                        leftIndex++;
                        continue;
                    }

                    string name = ArgumentName();

                    if (text[leftIndex++] is not '=')
                        throw new TagException("Arguments must have a value assign to it.");

                    if (Argument(out string? argument) && !string.IsNullOrWhiteSpace(name))
                        yield return (name, argument!);
                }
            }

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                if (text[leftIndex] is '<' && (!HasPrevious() || text[leftIndex - 1] is not '\\'))
                {
                    beginIndex = leftIndex;

                    if (leftIndex + 1 >= text.Length)
                        throw new TagException("Must end < with >.");

                    for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
                    {
                        if (text[rightIndex] is '<' && text[rightIndex - 1] is not '\\') // Invalid: must end with >. <tag> ! <tag<
                            throw new TagException("Not allowed to have two < before close with >.");

                        if (text[rightIndex] is '>' && text[rightIndex - 1] is not '\\')
                            break;

                        if (rightIndex + 1 >= text.Length)
                            throw new TagException("Must end < with >.");
                    }

                    leftIndex++;
                    rightIndex--;

                    if (text[leftIndex] is '/' && text[rightIndex] is '/')   // Invalid: not allowed to have both. ! </tag/>
                        throw new TagException("Not allowed to have both '/' on the beginning and the end of the tag.");

                    endIndex = rightIndex;

                    state = TagState.Open; // Open by default if it has no '/'.
                    if (text[leftIndex] is '/')    // Tag is an tag marker. </tag>
                    {
                        state = TagState.Marker;
                        leftIndex++;
                    }
                    else if (text[rightIndex] is '/')    // Tag is an closure tag. <tag/>
                    {
                        state = TagState.Close;
                        rightIndex--;
                    }

                    if (!TagName(out string tagName))
                    {
                        leftIndex = endIndex + 1;
                        continue;
                    }

                    string? implicitArgument = null;
                    if (text[leftIndex++] is '=')   // If it has implied arguments.
                        if (!Argument(out implicitArgument))
                        {
                            leftIndex = endIndex + 1;
                            continue;
                        }

                    if (state == TagState.Open)
                    {
                        Tag tag = (Tag)Activator.CreateInstance(Types[tagName]);
                        tag._range = new RangeInt(newLength, -1);

                        if (implicitArgument != null)
                            tag.Bind("implicit", implicitArgument);

                        foreach ((string name, string argument) in Arguments())
                            tag.Bind(name, argument);

                        tagOpenList.Add(tag);
                        tag.OnCreate();
                        yield return tag;
                    }
                    else if (state == TagState.Close)
                    {
                        int tagIndex = tagOpenList.FindLastIndex(0, x => x.GetType() == Types[tagName]);

                        if (tagIndex == -1)    // No open tag of that type exist.
                            throw new TagException("Closure tags must have an open tag of the same type.");

                        var range = tagOpenList[tagIndex]._range;
                        range.length = newLength - range.start;
                        tagOpenList[tagIndex]._range = range;

                        tagOpenList.RemoveAt(tagIndex);    // Remove first last index of a tag.
                    }
                    else if (state == TagState.Marker)
                    {
                        TagMarker tag = (TagMarker)Activator.CreateInstance(Types[tagName]);
                        tag._index = newLength;

                        if (implicitArgument != null)
                            tag.Bind("implicit", implicitArgument);

                        foreach ((string name, string argument) in Arguments())
                            tag.Bind(name, argument);

                        tag.OnCreate();
                        yield return tag;
                    }
                }
                else
                    newLength++;
            }
        }

        public static int TextLengthWithoutTags(string text)
        {
            int newLength = 0;
            int endIndex = 0;
            int beginIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;

            bool HasNext() => leftIndex + 1 < text.Length;
            bool HasPrevious() => leftIndex - 1 >= 0;

            bool TagName()
            {
                int beforeIndex = leftIndex;

                for (; leftIndex <= rightIndex; leftIndex++) // <?=
                {
                    if (text[leftIndex] is ' ' or '=')    // Ends if it finds a whitespace or =.
                        break;

                    if (!char.IsLetter(text[leftIndex]))    // Invalid: name must be a letter. <tag> ! <%3->
                        return false;
                }

                return true;    // Name is valid.
            }

            bool ValidTag()
            {
                if (leftIndex + 1 >= text.Length)
                    return false;

                for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
                {
                    if (text[rightIndex] is '<' && text[rightIndex - 1] is not '\\') // Invalid: must end with >. <tag> ! <tag<
                        return false;

                    if (text[rightIndex] is '>' && text[rightIndex - 1] is not '\\')
                        break;

                    if (rightIndex + 1 >= text.Length)
                        return false;
                }

                leftIndex++;
                rightIndex--;

                if (text[leftIndex] is '/' && text[rightIndex] is '/')   // Invalid: not allowed to have both. ! </tag/>
                    return false;

                return true;
            }

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                switch (text[leftIndex])
                {
                    case '\\' when !HasNext() || text[leftIndex + 1] is not '\\':
                        break;
                    case '<' when !HasPrevious() || text[leftIndex - 1] is not '\\':
                        beginIndex = leftIndex;

                        if (!ValidTag())
                        {
                            newLength++;
                            break;
                        }

                        endIndex = rightIndex + 1;

                        if (text[leftIndex] is '/')    // Tag is an tag marker. </tag>
                            leftIndex++;
                        else if (text[rightIndex] is '/')    // Tag is an closure tag. <tag/>
                            rightIndex--;

                        if (TagName())
                        {
                            leftIndex = endIndex;
                        }
                        else
                        {
                            for (leftIndex = beginIndex; leftIndex <= endIndex; leftIndex++)
                                newLength++;
                            leftIndex--;
                        }
                        break;
                    default:
                        newLength++;
                        break;
                }
            }

            return newLength;
        }

        public static string TrimTextTags(string text, bool onlyExistingTags = true)
        {
            Span<char> span = stackalloc char[text.Length];
            int newLength = 0;
            int endIndex = 0;
            int beginIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;

            bool TagName()
            {
                int beforeIndex = leftIndex;

                for (; leftIndex <= rightIndex; leftIndex++) // <?=
                {
                    if (text[leftIndex] is ' ' or '=')    // Ends if it finds a whitespace or =.
                        break;

                    if (!char.IsLetter(text[leftIndex]))    // Invalid: name must be a letter. <tag> ! <%3->
                        return false;
                }

                if (onlyExistingTags)
                {
                    string slice = text.Substring(beforeIndex, leftIndex - beforeIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

                    if (!Types.TryGetValue(slice, out Type type))   // If the tag does not exist.
                        return false;
                }

                return true;    // Name is valid.
            }

            bool ValidTag()
            {
                if (leftIndex + 1 >= text.Length)
                    return false;

                for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
                {
                    if (text[rightIndex] is '<' && text[rightIndex - 1] is not '\\') // Invalid: must end with >. <tag> ! <tag<
                        return false;

                    if (text[rightIndex] is '>' && text[rightIndex - 1] is not '\\')
                        break;

                    if (rightIndex + 1 >= text.Length)
                        return false;
                }

                leftIndex++;
                rightIndex--;

                if (text[leftIndex] is '/' && text[rightIndex] is '/')   // Invalid: not allowed to have both. ! </tag/>
                    return false;

                return true;
            }

            bool HasNext() => leftIndex + 1 < text.Length;
            bool HasPrevious() => leftIndex - 1 >= 0;

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                switch (text[leftIndex])
                {
                    case '\\' when !HasNext() || text[leftIndex + 1] is not '\\':
                        break;
                    case '<' when !HasPrevious() ||text[leftIndex - 1] is not '\\':
                        beginIndex = leftIndex;

                        if (!ValidTag())
                        {
                            span[newLength++] = '<';
                            break;
                        }

                        endIndex = rightIndex + 1;

                        if (text[leftIndex] is '/')    // Tag is an tag marker. </tag>
                            leftIndex++;
                        else if (text[rightIndex] is '/')    // Tag is an closure tag. <tag/>
                            rightIndex--;

                        if (TagName())
                        {
                            leftIndex = endIndex;
                        }
                        else
                        {
                            for (leftIndex = beginIndex; leftIndex <= endIndex; leftIndex++)
                                span[newLength++] = text[leftIndex];
                            leftIndex--;
                        }
                        break;
                    default:
                        span[newLength++] = text[leftIndex];
                        break;
                }
            }

            return span.Slice(0, newLength).ToString();
        }

        //private static bool ExtractTagName(out string name)
        //{
        //    name = string.Empty;
        //    int beforeIndex = leftIndex;

        //    for (; leftIndex <= rightIndex; leftIndex++) // <?=
        //    {
        //        if (text[leftIndex] is ' ' or '=')    // Ends if it finds a whitespace or =.
        //            break;

        //        if (!char.IsLetter(text[leftIndex]))    // Invalid: name must be a letter. <tag> ! <%3->
        //            throw new TagException("Name cannot contain any numbers or symbols");
        //    }

        //    string slice = text.Substring(beforeIndex, leftIndex - beforeIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

        //    if (!Types.TryGetValue(slice, out Type type))   // If the tag does not exist.
        //        return false;

        //    if ((typeof(Tag).IsAssignableFrom(type) && state is not TagState.Open and not TagState.Close) ||
        //        (typeof(TagMarker).IsAssignableFrom(type) && state is not TagState.Marker))
        //        throw new TagException($"Tag type: {type} is not derived from Tag");

        //    name = slice;

        //    return true;    // Name is valid.
        //}

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
    }

    public class TagException : Exception
    {
        public TagException(string message) : base(message) { }
    }
}
