using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.VersionControl;
using UnityEngine.Assertions;

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
                    UpdateTrimmedText();

                return _trimmedText!;
            }
            private set
            {
                _text = value;
                _trimmedText = Tags.TrimTextTags(_text, Tags.TagVariation.Custom);
                _length = Tags.GetTextLength(_trimmedText);
#if UNITY_EDITOR
                EditorUtility.IsDirty(this);
#endif
            }
        }

        public int Length => _length;

        public double StartTime => Clip.start;
        public double EndTime => Clip.end;
        public float TimeLength => (float)(EndTime - StartTime);

        /// <summary>
        /// How much time is left in unit interval [0-1]. Unaffected by speed.
        /// </summary>
        public float IntervalUnscaled =>
            Mathf.Clamp01((float)((Director.time - StartTime) / TimeLength));

        /// <summary>
        /// How much time is left dependent on speed in unit interval [0-1]. 0 if before and 1 if after.
        /// </summary>
        public float Interval =>
            VisibilityCurve.Evaluate(Mathf.Clamp01((float)((Director.time - StartTime - StartOffset) / (TimeLength - EndOffset - StartOffset))));

        public AnimationCurve VisibilityCurve
        {
            get => _visibilityCurve;
            set
            {
                _visibilityCurve = value;

                if (_runtimeVisibilityCurve == null)
                    _runtimeVisibilityCurve = new AnimationCurve(_visibilityCurve.keys);
                else
                    _runtimeVisibilityCurve.keys = _visibilityCurve.keys;
            }
        }

        public AnimationCurve RuntimeVisibilityCurve
        {
            get
            {
                if (_runtimeVisibilityCurve == null)
                    _runtimeVisibilityCurve = new AnimationCurve(_visibilityCurve.keys);

                return _runtimeVisibilityCurve;
            }
            set => _runtimeVisibilityCurve = value;
        }

        public float StartOffset
        {
            get => _startOffset;
            set => _startOffset = Mathf.Clamp(value, 0, TimeLength - _endOffset);
        }

        public float EndOffset
        {
            get => _endOffset;
            set => _endOffset = Mathf.Clamp(value, 0, TimeLength - _startOffset);
        }

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
        private AnimationCurve _visibilityCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField, HideInInspector]
        private int _length;

        private AnimationCurve? _runtimeVisibilityCurve;

        public void UpdateTrimmedText() => Text = _text;

        public int GetIndexByTime(double time)
        {
            float interval = VisibilityCurve.Evaluate(Mathf.Clamp01((float)((time - StartTime + StartOffset) / (EndTime - EndOffset - StartTime))));
            return Mathf.CeilToInt(interval * Tags.GetTextLength(Text));
        }

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
