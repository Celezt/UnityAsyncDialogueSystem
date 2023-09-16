using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public static class CollectionExtensions
    {
        // https://stackoverflow.com/a/28597288/12707382
        public static bool Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            // Exit if positions are equal or outside array.
            if ((oldIndex == newIndex) || (0 > oldIndex) || (oldIndex >= list.Count) 
                || (0 > newIndex) || (newIndex >= list.Count)) 
                return false;

            var i = 0;
            T tmp = list[oldIndex];
            // Move element down and shift other elements up.
            if (oldIndex < newIndex)
            {
                for (i = oldIndex; i < newIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            // Move element up and shift other elements down.
            else
            {
                for (i = oldIndex; i > newIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }
            // Put element from position 1 to destination.
            list[newIndex] = tmp;

            return true;
        }
    }
}
