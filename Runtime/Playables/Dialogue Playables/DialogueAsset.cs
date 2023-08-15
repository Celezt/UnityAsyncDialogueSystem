using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

namespace Celezt.DialogueSystem
{
    public class DialogueAsset : DSPlayableAsset, ITime
    {
        public string Actor
        {
            get => _actor;
            set => _actor = value;
        }

        public string RawText
        {
            get => _text;
            set
            {
                if (_text != value)
                    Text = value;
            }
        }

        public string Text
        {
            get
            {
                if (_trimmedText == null)
                    Text = _text;

                return _trimmedText!;
            }
            set
            {
                _text = value;
                RefreshDialogue();
            }
        }

        public int Length => _length;

        public IReadOnlyList<ITag> TagSequence
        {
            get
            {
                if (_tagSequence == null)
                {
                    RuntimeVisibilityCurve.keys = EditorVisibilityCurve.keys;
                    _tagSequence = Tags.GetTagSequence(RawText, this);
                }

                return _tagSequence;
            }
        }
        public double StartTime => Clip.start;
        public double EndTime => Clip.end;
        public float TimeDuration => (float)(EndTime - StartTime);
        public float TimeDurationWithoutOffset => TimeDuration - EndOffset - StartOffset;

        public int Index => GetIndexByTime(Director.time);

        public float Tangent => GetTangentByTime(Director.time);

        /// <summary>
        /// How much time has passed in unit interval [0-1].
        /// </summary>
        public float Interval => GetIntervalByTime(Director.time);

        /// <summary>
        /// The ratio of visible characters in unit interval [0-1].
        /// </summary>
        public float VisibilityInterval => GetVisibilityByTime(Director.time);

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

        public float StartOffset
        {
            get => _startOffset;
            set
            {
                if (_startOffset == value)
                    return;

                _startOffset = Mathf.Clamp(value, 0, TimeDuration - _endOffset);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
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

                _endOffset = Mathf.Clamp(value, 0, TimeDuration - _startOffset);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

#if UNITY_EDITOR
        internal bool HasUpdated { get; set; }
#endif

        [SerializeField]
        private string _actor = string.Empty;
        [SerializeField, TextArea(10, int.MaxValue)]
        private string _text = string.Empty;
        [SerializeField, HideInInspector]
        private string _trimmedText = string.Empty;
        [SerializeField, HideInInspector, Min(0)]
        private float _startOffset;
        [SerializeField, HideInInspector, Min(0)]
        private float _endOffset;
        [SerializeField, HideInInspector]
        private AnimationCurve _editorVisibilityCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField, HideInInspector]
        private int _length;

        private AnimationCurve? _runtimeVisibilityCurve;
        private List<ITag>? _tagSequence;

        public void RefreshDialogue()
        {
            RuntimeVisibilityCurve.keys = EditorVisibilityCurve.keys;

            _trimmedText = Tags.TrimTextTags(_text, Tags.TagVariation.Custom);
            _length = Tags.GetTextLength(_trimmedText);
            _tagSequence = Tags.GetTagSequence(RawText, this);
#if UNITY_EDITOR
            EditorUtility.IsDirty(this);
            HasUpdated = true;
#endif
        }

        public void UpdateTags()
        {
            RuntimeVisibilityCurve.keys = EditorVisibilityCurve.keys;

            if (_tagSequence != null)
                Tags.InvokeAll(TagSequence);
            else
                _tagSequence = Tags.GetTagSequence(RawText, this);
        }

        public float GetVisibilityByTime(double time, CurveType curveType = CurveType.Runtime)
            => GetVisibilityByTimeInterval(GetIntervalByTime(time), curveType);

        public float GetIntervalByTime(double time) => Mathf.Clamp01((float)((time - StartTime - StartOffset) / TimeDurationWithoutOffset));

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
            => TryGetTimeByIndex(index, StartTime + StartOffset, TimeDurationWithoutOffset, out time, out visibility, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, double startTime, double duration, out double time, out float visibility, CurveType curveType = CurveType.Runtime)
        {
            double framerate = ((TimelineAsset)Director.playableAsset).editorSettings.frameRate;
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
            double framerate = ((TimelineAsset)Director.playableAsset).editorSettings.frameRate;
            float previousValue = 0;
            double time = startTime;
            int count = 0;

            while (time < startTime + duration && intersections.Count < count)
            {
                float visibility = GetVisibilityByTime(time, curveType);
                float currentValue = visibility * Length;

                if (index == 0 && StartOffset > 0 ? previousValue <= index && currentValue > index :
                                                    previousValue < index && currentValue >= index)
                    intersections[count++] = time;

                time += 1.0 / framerate;
            }

            return count;
        }

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
