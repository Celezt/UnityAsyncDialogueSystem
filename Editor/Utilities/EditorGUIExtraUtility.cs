using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    internal static class EditorGUIExtraUtility
    {
        public static void TightLabel(string labelStr)
        {
            GUIContent label = new GUIContent(labelStr);
            EditorGUILayout.LabelField(label, GUILayout.Width(GUI.skin.label.CalcSize(label).x));
        }
    }
}
