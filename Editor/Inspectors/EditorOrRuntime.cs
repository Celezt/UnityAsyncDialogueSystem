using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public static class EditorOrRuntime
    {
        public static bool IsRuntime
        {
            get => !_isEditor;
            set => _isEditor = !value;
        }

        public static bool IsEditor
        {
            get => _isEditor;
            set => _isEditor = value;
        }

        private static bool _isEditor = true;
    }
}
