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
    public class Tags
    {
        public static IReadOnlyDictionary<string, ITag> Values
        {
            get
            {
                if (_values == null)
                    Initialize();

                return _values!;
            }
        }

        private static Dictionary<string, ITag>? _values;
        private static Dictionary<ITag, Dictionary<string, MemberInfo>> _cachedMembers = new();

        private enum TagState
        {
            Open,
            Close,
            Marker
        }

        public static void BindTag(ITag tag, string name, string argument)
        {
            if (!_cachedMembers.TryGetValue(tag, out var members))
            {
                Type type = tag.GetType();
                _cachedMembers[tag] = members = type  // Get all public field, properties and fields with SerializeFieldAttribute.
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                    .Cast<MemberInfo>()
                    .Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.IsPrivate ? x.GetCustomAttribute<SerializeField>() != null : x.IsPublic))
                    .ToDictionary(key => key.Name.ToCamelCase(), value => value);
            }

            if (members.TryGetValue(name, out var member))
                member.SetValue(tag, argument);
        }

        public static IEnumerable<ITag> GetTags(string text)
        {
            int beginIndex = 0;
            int endIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;
            TagState state = TagState.Open;
            var tagOpenList = new List<string>();

            

            bool TagName(out string name)
            {
                name = string.Empty;
                int length = 0;

                for (; length < rightIndex - leftIndex; length++) // <?=
                {
                    if (text[leftIndex + length] is ' ' or '=')    // Ends if it finds a whitespace or =.
                        break;

                    if (!char.IsLetter(text[leftIndex + length]))    // Invalid: name must be a letter. <tag> ! <%3->
                        throw new TagException("Name cannot contain any numbers or symbols");
                }

                string slice = text.Substring(leftIndex, length);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

                if (!Values.ContainsKey(slice))   // If the tag does not exist.
                    return false;

                name = slice;
                leftIndex += length;
                return true;    // Name is valid.
            }

            bool Argument(out string? argument)
            {
                argument = null;

                if (state == TagState.Close)    // Invalid: no arguments allowed on a closing tag. <tag/> ! <tag=?/>
                    throw new TagException("Closure tags are not allowed to contain any arguments."); 

                int length = 0;
                char decoration = '\0';

                if (text[leftIndex + length] is '"' or '\'') // Ignore decoration.
                {
                    decoration = text[leftIndex++ + length++];

                    for (; length < rightIndex - leftIndex; length++)
                    {
                        // Argument ending except when having a '\' in front of it.
                        if (text[leftIndex + length] == decoration && text[leftIndex + length - 1] is not '\\')
                            break;

                        if (length >= rightIndex - leftIndex - 1)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                            throw new TagException("Arguments using \" or ' must close with the same character.");
                    }
                }
                else if (text[leftIndex + length] is not ' ')    // If no decorations are present. WARNING: whitespace means end!
                {
                    for (; length < rightIndex - leftIndex; length++)
                    {
                        if (char.IsWhiteSpace(text[leftIndex + length]))
                            break;

                        // Invalid: not allowed to use these characters except when having a '\' in front of it.
                        if (text[leftIndex + length] is '/' or '"' or '\'' && text[leftIndex + length - 1] is not '\\')
                            throw new TagException("Not allowed to use /, \" or ' except when having a \\ in front of it.");
                    }
                }
                else
                    throw new TagException("Arguments cannot be empty");

                argument = text.Substring(leftIndex, length);
                leftIndex += length + (decoration is '\0' ? 0: 1);

                return true;
            }

            IEnumerable<(string Name, string Argument)> Arguments()
            {
                string ArgumentName()
                {
                    int length = 0;

                    for (; length < rightIndex - leftIndex; length++) // <?=
                    {
                        if (text[leftIndex + length] is '=')    // Ends if it finds a =.
                            break;

                        if (text[leftIndex + length] is ' ')
                            throw new TagException("Argument names are not allowed to end with whitespace.");

                        if (!char.IsLetter(text[leftIndex + length]))    // Invalid: name must be a letter. <tag> ! <%3->
                            throw new TagException("Name cannot contain any numbers or symbols");
                    }

                    string slice = text.Substring(leftIndex, length);

                    leftIndex += length;
                    return slice;
                }

                while(leftIndex < rightIndex)
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
                if (text[leftIndex] is '<')
                {
                    beginIndex = leftIndex;

                    for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
                    {
                        if (text[rightIndex] is '<') // Invalid: must end with >. <tag> ! <tag<
                            break;

                        if (text[rightIndex] is not '>')
                            continue;

                        endIndex = rightIndex;

                        if (text[leftIndex + 1] is '/' && text[rightIndex - 1] is '/')   // Invalid: not allowed to have both. ! </tag/>
                            break;

                        state = TagState.Open; // Open by default if it has no '/'.
                        if (text[leftIndex + 1] is '/')    // Tag is an tag marker. </tag>
                        {
                            Debug.Log("marker!");
                            state = TagState.Marker;
                            leftIndex += 2;
                        }
                        else if (text[rightIndex - 1] is '/')    // Tag is an closure tag. <tag/>
                        {
                            Debug.Log("close!");
                            state = TagState.Close;
                            rightIndex -= 2;
                        }
                        else
                            Debug.Log("open!");

                        if (!TagName(out string name))
                            break;

                        Debug.Log("Name: " + name);

                        string? implicitArgument = null;
                        if (text[leftIndex++] is '=')   // If it has implied arguments.
                            if (!Argument(out implicitArgument))
                                break;
      
                        Debug.Log("Implied: " + implicitArgument);

                        foreach (var argument in Arguments())
                        {
                            Debug.Log(argument);
                        }

                        if (state == TagState.Open)
                        {
                            tagOpenList.Add(name);
                            yield return Values[name];
                        }
                        else if (state == TagState.Close)
                        {
                            int index = tagOpenList.LastIndexOf(name);

                            if (index == -1)    // No open tag of that type exist.
                                break;

                            tagOpenList.RemoveAt(index);    // Remove first last index of a tag.
                        }
                        else if (state == TagState.Marker)
                        {

                            yield return Values[name];
                        }
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            _values = new();

            foreach (Type tagType in ReflectionUtility.GetTypesWithAttribute<CreateTagAttribute>(AppDomain.CurrentDomain))
            {
                if (tagType.GetInterface(nameof(ITag)) == null)
                    throw new Exception("Object with 'CreateTagAttribute' are required to be derived from 'ITag'");

                Span<char> span = stackalloc char[tagType.Name.Length];
                string name = tagType.Name.TrimDecorationSpan(span, "Tag").ToCamelCaseSpan().ToString();
                _values[name] = (ITag)Activator.CreateInstance(tagType);

                Debug.Log("Added: " + name);
            }
        }
    }

    public class TagException : Exception
    {
        public TagException(string message) : base(message) { }
    }
}
