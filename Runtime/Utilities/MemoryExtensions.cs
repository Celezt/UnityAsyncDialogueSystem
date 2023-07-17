using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    internal static class MemoryExtensions
    {
        public static bool Any(this Span<bool> span)
        {
            for (int i = 0; i < span.Length; i++)
                if (span[i] == true)
                    return true;

            return false;
        }
    }
}
