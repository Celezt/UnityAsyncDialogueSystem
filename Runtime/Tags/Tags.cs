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
            var tagOpenList = new List<string>();

            bool NameProcess(out string name)
            {
                int index = leftIndex;
                name = string.Empty;

                for (; index < rightIndex; index++) // <?=
                {
                    if (text[index] is ' ' or '=')    // Ends if it finds a whitespace or =.
                        break;

                    if (!char.IsLetter(text[index]))    // Invalid: name must be a letter. <tag> ! <%3->
                        return false;
                }

                string slice = text.Substring(leftIndex, index - leftIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

                if (!Values.ContainsKey(slice))   // If the tag does not exist.
                    return false;

                name = slice;
                leftIndex = index;
                return true;    // Name is valid.
            }

            bool ImpliedProcess(TagState state, out ReadOnlySpan<char> impliedArgument)
            {
                impliedArgument = ReadOnlySpan<char>.Empty;

                if (text[leftIndex++] is not '=')   // If it has no implied arguments.
                    return true;

                if (state == TagState.Close)    // Invalid: no arguments allowed on a closing tag. <tag/> ! <tag=?/>
                    return false;

                char decoration = '\0';
                int length = 0;

                if (text[leftIndex + length] is '"' or '\'') // Ignore decoration.
                {
                    decoration = text[leftIndex++ + length++];

                    for (; length < rightIndex - leftIndex; length++)
                    {
                        // Argument ending except when having a '\' in front of it.
                        if (text[leftIndex + length] == decoration && text[leftIndex + length - 1] is not '\\')
                            break;

                        if (length >= rightIndex - leftIndex - 1)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                            return false;
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
                            return false;
                    }
                }
                else
                    return false;

                impliedArgument = text.AsSpan(leftIndex, length);
                leftIndex += length + 1;

                return true;
            }

            //bool ArgumentProcess(TagState state)
            //{

            //    for (; leftIndex < rightIndex; leftIndex++)
            //    {
            //        if (!char.IsWhiteSpace(text[leftIndex])) // Skip all white spaces.
            //            break;
            //    }

            //    int index = leftIndex;

            //    for (; index < rightIndex; index++)
            //    {
            //        if ()
            //    }

            //    for (int i = leftIndex + 1; i < rightIndex; i++) // =?>
            //    {

            //    }

            //    return true;
            //}

            for (leftIndex = 0; leftIndex < text.Length; leftIndex++)
            {
                //Debug.Log(text[leftIndex]);
                if (text[leftIndex] is '<')
                {
                    beginIndex = leftIndex;
                    Debug.Log("Begin Index: " + beginIndex);
                    for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
                    {
                        if (text[rightIndex] is '<') // Invalid: must end with >. <tag> ! <tag<
                            break;

                        if (text[rightIndex] is not '>')
                            continue;

                        endIndex = rightIndex;
                        Debug.Log("End Index: " + endIndex);
                        if (text[leftIndex + 1] is '/' && text[rightIndex - 1] is '/')   // Invalid: not allowed to have both. ! </tag/>
                            break;

                        TagState state = TagState.Open; // Open by default if it has no '/'.
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

                        if (!NameProcess(out string name))
                            break;

                        Debug.Log("Name: " + name);

                        if (!ImpliedProcess(state, out ReadOnlySpan<char> impliedArgument))
                            break;

                        Debug.Log("Implied: " + impliedArgument.ToString());

                        //if (!ArgumentProcess(state))
                        //    break;

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
}
