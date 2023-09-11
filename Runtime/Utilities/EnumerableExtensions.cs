using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public static class EnumerableExtensions
    {
        public static int FindIndex<T>(this IEnumerable<T> obj, Predicate<T> match)
        {
            int index = 0;
            foreach (var element in obj)
            {
                if (match(element))
                    return index;

                index++;
            }

            return index;
        }

        public static int IndexOf<T>(this IEnumerable<T> obj, T value) => obj.IndexOf(value, null);
        public static int IndexOf<T>(this IEnumerable<T> obj, T value, IEqualityComparer<T> comparer)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            var found = obj
                .Select((a, i) => new { a, i })
                .FirstOrDefault(x => comparer.Equals(x.a, value));
            return found == null ? -1 : found.i;
        }

        public static bool AddRange<T>(this HashSet<T> obj, IEnumerable<T> elements)
        {
            bool isAnyExisting = false;
            foreach (var element in elements)
                isAnyExisting |= !obj.Add(element);

            return isAnyExisting;
        }
    }
}
