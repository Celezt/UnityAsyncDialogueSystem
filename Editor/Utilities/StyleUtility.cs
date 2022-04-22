using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class StyleUtility
    {
        internal const string STYLE_PATH = "Packages/com.celezt.asyncdialogue/Editor/Resources/Styles/";

        /// <summary>
        /// Adds a style sheet from path to the owner element.
        /// </summary>
        /// <param name="element">Element to own the style sheet.</param>
        /// <param name="styleSheetPath">Path to the style sheet.</param>
        public static VisualElement AddStyleSheet(this VisualElement element, string styleSheetPath)
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath + ".uss");

            if (styleSheet == null)
                throw new NullReferenceException(styleSheetPath + ".uss not found.");
            if (!element.styleSheets.Contains(styleSheet))
                element.styleSheets.Add(styleSheet);
            return element;
        }
    }
}
