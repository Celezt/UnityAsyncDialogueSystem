using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public static class EditorGUIExtra
    {
        public struct Modification : IDisposable
        {
            private Func<Rect> _onGetRect;
            private SerializedProperty _property;
            private IExtension _extension;
            private Action _onHasChange;

            public Modification(Func<Rect> onGetRect, SerializedProperty property, IExtension extension, Action onHasChange)
            {
                _onGetRect = onGetRect;
                _property = property;
                _extension = extension;
                _onHasChange = onHasChange;
            }

            public void Dispose()
            {
                // If content in current property is not the same as reference property. 
                if (_extension.GetModified(_property.name))
                {
                    Rect rect = _onGetRect();
                    rect.x = 0;
                    rect.width = 2;

                    EditorGUI.DrawRect(rect, new Color(0.06f, 0.50f, 0.75f));
                }

                if (EditorGUI.EndChangeCheck())
                    _onHasChange?.Invoke();
            }

            public static Modification Scope(Func<Rect> onGetRect, SerializedProperty property, IExtension extension, Action onHasChange = null)
            {
                EditorGUI.BeginChangeCheck();

                return new Modification(onGetRect, property, extension, onHasChange);
            }
        }

        public struct Disable : IDisposable
        {
            public Disable(bool disable)
            {
                GUI.enabled = disable;
            }

            public void Dispose()
            {
                GUI.enabled = true;
            }

            public static Disable Scope()
                => new Disable(false);

            public static Disable Scope(bool disable)
                => new Disable(!disable);
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
