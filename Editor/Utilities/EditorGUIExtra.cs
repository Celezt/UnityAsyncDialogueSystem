using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public static class EditorGUIExtra
    {
        public static void DrawCurve(Rect rect, Color color, AnimationCurve curve, int subdivitions = 8)
        {
            if (curve.length < 1)
                return;

            int iterations = (int)(Mathf.Log(rect.width) * subdivitions);
            float previousTime = 0;
            float previousValue = 0;

            Handles.color = color;
            for (int i = 0; i <= iterations; i++)
            {
                float time = i / (float)iterations;
                float value = curve.Evaluate(time);

                Handles.DrawAAPolyLine(3,
                    new Vector3(previousTime * rect.width + rect.position.x, (1 - previousValue) * rect.height + rect.position.y, 0),
                    new Vector3(time * rect.width + rect.position.x, (1 - value) * rect.height + rect.position.y, 0));

                previousTime = time;
                previousValue = value;
            }
        }
    }

    public static class EditorGUILayoutExtra
    {
        public static Color EditorColor => EditorGUIUtility.isProSkin ? DarkColor : LightColor;

        public static readonly Color LightColor = new Color32(194, 194, 194, 255);
        public static readonly Color DarkColor = new Color32(56, 56, 56, 255);

        public static void CurveField(AnimationCurve curve, Color curveColor, Rect ranges, int subdivitions = 8)
            => CurveField(curve, curveColor, EditorColor, ranges, subdivitions);
        public static void CurveField(AnimationCurve curve, Color curveColor, Color backgroundColor, Rect ranges, int subdivitions = 8)
        {
            EditorGUILayout.CurveField(curve, curveColor, ranges);
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(rect, backgroundColor);
            EditorGUIExtra.DrawCurve(rect, curveColor, curve);
        }
    }
}
