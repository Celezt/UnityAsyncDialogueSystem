using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class DSStyleUtility
    {
        public const string STYLE_PATH = "Packages/com.celezt.asyncdialogue/Editor/Resources/Styles/";

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
                element.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_PATH + styleSheetName + ".uss"));

            return element;
        }
    }
}
