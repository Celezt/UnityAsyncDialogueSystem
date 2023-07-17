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
        public static IReadOnlyDictionary<string, ITag> Instances => _tagInstances;

        private static Dictionary<string, ITag> _tagInstances = new();
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

            //if (members.TryGetValue(name, ))

            //foreach (MemberInfo member in members) 
            //{
            //    member.SetValue(name, argument);
            //}
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
                    if (char.IsWhiteSpace(text[index]) || text[index] == '=')    // Ends if it finds a whitespace or =.
                        break;

                    if (!char.IsLetter(text[index]))    // Invalid: name must be a letter. <tag> ! <%3->
                        return false;
                }

                string slice = text.Substring(leftIndex, index - leftIndex);   // PLEASE LET US COMPARE DICTIONARY WITH A IREADONLYSPAN!!!  

                if (!Instances.ContainsKey(slice))   // If the tag does not exist.
                    return false;

                name = slice;
                leftIndex = index;
                return true;    // Name is valid.
            }

            bool ImpliedProcess(TagState state, out ReadOnlySpan<char> impliedArgument)
            {
                impliedArgument = string.Empty;
                int index = leftIndex + 1;
                char decoration = '\0';

                if (text[leftIndex] is '=')   // If it has an implied argument.
                {
                    leftIndex++;

                    if (state == TagState.Close)    // Invalid: no arguments allowed on a closing tag. <tag/> ! <tag=?/>
                        return false;

                    if (text[index] is '"' or '\'') // Ignore decoration.
                        decoration = text[index++];

                    if (decoration is '\0') // If no decorations are present. WARNING: whitespace means end!
                    {
                        for (; index <= rightIndex; index++)
                        {
                            if (char.IsWhiteSpace(text[index]))
                            {
                                index--;
                                break;
                            }

                            // Invalid: not allowed to use these characters except when having a '\' in front of it.
                            if (text[index] is '/' or '"' or '\'' && text[index - 1] is not '\\')
                                return false;
                        }

                        impliedArgument = text.AsSpan(leftIndex, index - leftIndex);
                    }
                    else
                    {
                        for (; index <= rightIndex; index++)
                        {
                            // Argument ending except when having a '\' in front of it.
                            if (text[index] == decoration && text[index - 1] is not '\\') 
                                break;

                            if (index >= rightIndex - 1)    // Invalid: must have a closure. <tag="?"> ! <tag="?>
                                return false;
                        }

                        impliedArgument = text.AsSpan(leftIndex + 1, index - leftIndex - 1);    // Slice without decorators.
                    }

                    leftIndex = index + 1;
                }

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

            for (int i = 0; i < text.Length; i++)
            {
                if (text[leftIndex] == '<')
                {
                    leftIndex = beginIndex = i;
                    for (rightIndex = leftIndex + 1; rightIndex < text.Length; rightIndex++)
                    {
                        if (text[rightIndex] == '<') // Invalid: must end with >. <tag> ! <tag<
                            break;

                        if (text[rightIndex] == '>') // End of tag. <tag>
                        {
                            endIndex = rightIndex;

                            if (text[leftIndex + 1] == '/' && text[rightIndex - 1] == '/')   // Invalid: not allowed to have both. ! </tag/>
                                break;

                            TagState state = TagState.Open; // Open by default if it has no '/'.
                            if (text[leftIndex + 1] == '/')    // Tag is an tag marker. </tag>
                            {
                                state = TagState.Marker;
                                leftIndex += 2;
                            }
                            else if (text[rightIndex - 1] == '/')    // Tag is an closure tag. <tag/>
                            {
                                state = TagState.Close;
                                rightIndex -= 2;
                            }

                            if (!NameProcess(out string name))
                                break;

                            if (!ImpliedProcess(state, out ReadOnlySpan<char> impliedArgument))
                                break;

                            //if (!ArgumentProcess(state))
                            //    break;

                            if (state == TagState.Open)
                            {
                                tagOpenList.Add(name);
                                yield return null;
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

                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            foreach (Type tagType in ReflectionUtility.GetTypesWithAttribute<CreateTagAttribute>(AppDomain.CurrentDomain))
            {
                if (tagType.GetInterface(nameof(ITag)) == null)
                    throw new Exception("Object with 'CreateTagAttribute' are required to be derived from 'ITag'");

                Span<char> span = stackalloc char[tagType.Name.Length];
                string name = tagType.Name.TrimDecorationSpan(span, "Tag").ToCamelCaseSpan().ToString();
                _tagInstances[name] = (ITag)Activator.CreateInstance(tagType);

                Debug.Log("Added: " + name);
            }
        }
    }
}
