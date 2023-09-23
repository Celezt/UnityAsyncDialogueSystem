using Celezt.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Celezt.DialogueSystem
{
    public interface ITagText
    {
        public IReadOnlyList<ITag> TagSequence { get; }
        public int Length { get; }
        public string EditorText { get; set; }
        public MutString RuntimeText { get; }

        public void RefreshText();

        public bool TryInsertAfter(int visibleIndex, ReadOnlySpan<char> span, bool isWhitespaceAllowed = true)
        {
            visibleIndex++;

            int index = Tags.GetIndexFromVisibleIndex(RuntimeText, visibleIndex, out char character, out _);

            if (index < 0)
                return false;

            if (!isWhitespaceAllowed && character is ' ')
                return false;

            RuntimeText.Insert(index, span);

            return true;
        }

        public bool TryInsertBefore(int visibleIndex, ReadOnlySpan<char> span, bool isWhitespaceAllowed = true)
        {
            char character = '\0';
            int index = visibleIndex == 0 ? 0 : Tags.GetIndexFromVisibleIndex(RuntimeText, visibleIndex, out _, out character);

            if (index < 0)
                return false;

            if (!isWhitespaceAllowed && character is ' ')
                return false;

            RuntimeText.Insert(index, span);

            return true;
        }
    }
}
