using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITag
    {
        public object? Bind { get; }

        /// <summary>
        /// Shrinks the visibility curve.
        /// </summary>
        public void AddPadding(float padding);
    }
}
