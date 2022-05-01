using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public static class EnumerableExtension
    {
        public static int IndexOf<T>(this IEnumerable<T> obj, T value) => obj.IndexOf(value, null);
        public static int IndexOf<T>(this IEnumerable<T> obj, T value, IEqualityComparer<T> comparer)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            var found = obj
                .Select((a, i) => new { a, i })
                .FirstOrDefault(x => comparer.Equals(x.a, value));
            return found == null ? -1 : found.i;
        }
    }
}
