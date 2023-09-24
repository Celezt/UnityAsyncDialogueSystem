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
        public struct Disable : IDisposable
        {
            public void Dispose()
            {
                GUI.enabled = true;
            }

            public static Disable Scope()
            {
                GUI.enabled = false;
                return new Disable();
            }

            public static Disable Scope(bool disable)
            {
                GUI.enabled = !disable;
                return new Disable();
            }
        }

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

        // https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-8546099
        public static void DrawUILine(Color color = default, int thickness = 1, int padding = 10, int margin = 0)
        {
            color = color != default ? color : Color.grey;
            Rect r = EditorGUILayout.GetControlRect(false, GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding * 0.5f;

            switch (margin)
            {
                // Expand to maximum width.
                case < 0:
                    r.x = 0;
                    r.width = EditorGUIUtility.currentViewWidth;

                    break;
                case > 0:
                    // Shrink line width.
                    r.x += margin;
                    r.width -= margin * 2;

                    break;
            }

            EditorGUI.DrawRect(r, color);
        }
    }
}
