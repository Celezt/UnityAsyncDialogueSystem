using Celezt.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

#nullable enable

namespace Celezt.DialogueSystem
{
    [CreateExtension]
    public class TextExtension : Extension<DialogueAsset>
    {
        public int Index => GetIndexByTime(Asset.Director.time);

        public float Tangent => GetTangentByTime(Asset.Director.time);

        /// <summary>
        /// How much time has passed in unit interval [0-1].
        /// </summary>
        public float Interval => GetIntervalByTime(Asset.Director.time);

        /// <summary>
        /// The ratio of visible characters in unit interval [0-1].
        /// </summary>
        public float VisibilityInterval => GetVisibilityByTime(Asset.Director.time);

        public AnimationCurve EditorVisibilityCurve => _editorVisibilityCurve;

        public AnimationCurve RuntimeVisibilityCurve
        {
            get
            {
                if (_runtimeVisibilityCurve == null)
                    _runtimeVisibilityCurve = new AnimationCurve(_editorVisibilityCurve.keys);

                return _runtimeVisibilityCurve;
            }
        }

        public int Length => _length;

        public string EditorText
        {
            get => _editorText;
            set
            {
                _editorText = value;
                RefreshDialogue();
            }
        }

        public MutString RuntimeText
        {
            get
            {
                if (_runtimeText is null)
                {
                    _runtimeText = new MutString(_editorText.Length);
                    RefreshDialogue();
                }

                return _runtimeText;
            }
        }

        public float TimeDurationWithoutOffset => Asset.TimeDuration - EndOffset - StartOffset;

        public float StartOffset
        {
            get => _startOffset;
            set
            {
                if (_startOffset == value)
                    return;

                _startOffset = Mathf.Clamp(value, 0, Asset.TimeDuration - _endOffset);

#if UNITY_EDITOR
                EditorUtility.SetDirty(Asset);
#endif
            }
        }

        public float EndOffset
        {
            get => _endOffset;
            set
            {
                if (_endOffset == value)
                    return;

                _endOffset = Mathf.Clamp(value, 0, Asset.TimeDuration - _startOffset);

#if UNITY_EDITOR
                EditorUtility.SetDirty(Asset);
#endif
            }
        }

        [SerializeField, TextArea(10, int.MaxValue)]
        private string _editorText = string.Empty;
        [SerializeField, Min(0)]
        private float _startOffset;
        [SerializeField, Min(0)]
        private float _endOffset;
        [SerializeField]
        private AnimationCurve _editorVisibilityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private int _length;
        private MutString? _runtimeText;
        private AnimationCurve? _runtimeVisibilityCurve;
        private List<ITag>? _tagSequence;

        public void RefreshDialogue()
        {
            RuntimeVisibilityCurve.keys = EditorVisibilityCurve.keys;

            Span<char> span = stackalloc char[_editorText.Length];
            _editorText.AsSpan().CopyTo(span);

            _tagSequence = Tags.GetTagSequence(span, Asset);
            _length = Tags.GetTextLength(span);

            _runtimeText?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));
            Tags.InvokeAll(_tagSequence);
#if UNITY_EDITOR
            EditorUtility.IsDirty(Asset);
            Asset.HasUpdated = true;
#endif
        }

        public void UpdateTags()
        {
            RuntimeVisibilityCurve.keys = EditorVisibilityCurve.keys;

            Span<char> span = stackalloc char[_editorText.Length];
            _editorText.AsSpan().CopyTo(span);
            _runtimeText?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));

            _tagSequence ??= Tags.GetTagSequence(EditorText, this);

            Tags.InvokeAll(_tagSequence);
        }

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

        public float GetVisibilityByTime(double time, CurveType curveType = CurveType.Runtime)
            => GetVisibilityByTimeInterval(GetIntervalByTime(time), curveType);

        public float GetIntervalByTime(double time) => Mathf.Clamp01((float)((time - Asset.StartTime - StartOffset) / TimeDurationWithoutOffset));

        public float GetVisibilityByTimeInterval(float timeInterval, CurveType curveType = CurveType.Runtime) => curveType switch
        {
            CurveType.Editor => EditorVisibilityCurve.Evaluate(timeInterval),
            CurveType.Runtime => RuntimeVisibilityCurve.Evaluate(timeInterval),
            _ => throw new NotSupportedException($"{curveType} is not supported."),
        };

        public int GetIndexByTime(double time, CurveType curveType = CurveType.Runtime) => (int)Mathf.Round(GetVisibilityByTime(time, curveType) * Length);

        public float GetTangentByTime(double time, CurveType curveType = CurveType.Runtime)
        {
            float x1 = GetIntervalByTime(time - 0.001);
            float x2 = GetIntervalByTime(time + 0.001);
            float y1 = GetVisibilityByTimeInterval(x1, curveType);
            float y2 = GetVisibilityByTimeInterval(x2, curveType);
            return (y2 - y1) / (x2 - x1);
        }

        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, out double time, CurveType curveType = CurveType.Runtime)
            => TryGetTimeByIndex(index, out time, out _, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, double startTime, double duration, out double time, CurveType curveType = CurveType.Runtime)
            => TryGetTimeByIndex(index, startTime, duration, out time, out _, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, out double time, out float visibility, CurveType curveType = CurveType.Runtime)
            => TryGetTimeByIndex(index, Asset.StartTime + StartOffset, TimeDurationWithoutOffset, out time, out visibility, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, double startTime, double duration, out double time, out float visibility, CurveType curveType = CurveType.Runtime)
        {
            double framerate = ((TimelineAsset)Asset.Director.playableAsset).editorSettings.frameRate;
            float previousValue = 0;
            time = startTime;
            visibility = 0;

            while (time < startTime + duration)
            {
                visibility = GetVisibilityByTime(time, curveType);
                float currentValue = visibility * Length;

                if (index == 0 && StartOffset > 0 ? previousValue <= index && currentValue > index :
                                                    previousValue < index && currentValue >= index)
                    return true;

                time += 1.0 / framerate;
                previousValue = currentValue;
            }

            return false;
        }
        /// <summary>
        /// Get all intersections by index.
        /// </summary>
        /// <returns>Intersection count.</returns>
        public int GetTimeByIndex(int index, double startTime, double duration, IList<double> intersections, CurveType curveType = CurveType.Runtime)
        {
            double framerate = ((TimelineAsset)Asset.Director.playableAsset).editorSettings.frameRate;
            float previousValue = 0;
            double time = startTime;
            int count = 0;

            while (time < startTime + duration && intersections.Count < count)
            {
                float visibility = GetVisibilityByTime(time, curveType);
                float currentValue = visibility * Length;

                if (index == 0 && Asset.StartOffset > 0 ? previousValue <= index && currentValue > index :
                                                    previousValue < index && currentValue >= index)
                    intersections[count++] = time;

                time += 1.0 / framerate;
            }

            return count;
        }
    }
}
