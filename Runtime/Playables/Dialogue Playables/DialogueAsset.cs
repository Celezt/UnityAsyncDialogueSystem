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

                return _trimmedText;
            }
            private set
            {
                _text = value;
                _trimmedText = Tags.TrimTextTags(_text);
#if UNITY_EDITOR
                EditorUtility.IsDirty(this);
#endif
            }
        }

        public double StartTime => Clip.start;
        public double EndTime => Clip.end;
        public float Length => (float)(EndTime - StartTime);

        /// <summary>
        /// How much time is left in unit interval [0-1]. Unaffected by speed.
        /// </summary>
        public float IntervalUnscaled =>
            Mathf.Clamp01((float)((Director.time - StartTime) / Length));

        /// <summary>
        /// How much time is left dependent on speed in unit interval [0-1]. 0 if before and 1 if after.
        /// </summary>
        public float Interval =>
            TimeVisibilityCurve.Evaluate(Mathf.Clamp01((float)((Director.time - StartTime - StartOffset) / (Length - EndOffset - StartOffset))));

        public AnimationCurve TimeVisibilityCurve
        {
            get => _timeVisibilityCurve;
            set => _timeVisibilityCurve = value;
        }

        public float StartOffset
        {
            get => _startOffset;
            set => _startOffset = Mathf.Clamp(value, 0, Length - _endOffset);
        }

        public float EndOffset
        {
            get => _endOffset;
            set => _endOffset = Mathf.Clamp(value, 0, Length - _startOffset);
        }

        [SerializeField]
        private string _actor;
        [SerializeField, TextArea(10, int.MaxValue)]
        private string _text;
        [SerializeField, HideInInspector]
        private string _trimmedText;
        [SerializeField, HideInInspector, Min(0)]
        private float _startOffset;
        [SerializeField, HideInInspector, Min(0)]
        private float _endOffset;
        [SerializeField, HideInInspector]
        private AnimationCurve _timeVisibilityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public void UpdateTrimmedText() => Text = _text;

        public int GetIndexByTime(double time)
        {
            float interval = TimeVisibilityCurve.Evaluate(Mathf.Clamp01((float)((time - StartTime + StartOffset) / (EndTime - EndOffset - StartTime))));
            return Mathf.CeilToInt(interval * Tags.GetTextLength(Text));
        }

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
