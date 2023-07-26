using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.VersionControl;

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

        /// <summary>
        /// How much time is left in unit interval [0-1]. Unaffected by speed.
        /// </summary>
        public float IntervalUnscaled =>
            Mathf.Clamp01((float)((Director.time - StartTime) / (EndTime - StartTime)));

        /// <summary>
        /// How much time is left dependent on speed in unit interval [0-1]. 0 if before and 1 if after.
        /// </summary>
        public float Interval =>
            TimeSpeed.Evaluate(Mathf.Clamp01((float)((Director.time - StartTime + StartOffset) / (EndTime - EndOffset - StartTime))));


        [field: SerializeField]
        public AnimationCurve TimeSpeed { get; set; } = AnimationCurve.Linear(0, 0, 1, 1);
        [field: SerializeField, Min(0)]
        public float StartOffset { get; set; }
        [field: SerializeField, Min(0)]
        public float EndOffset { get; set; } = 1;

        [SerializeField]
        private string _actor;
        [SerializeField, TextArea(10, int.MaxValue)]
        private string _text;
        [SerializeField, HideInInspector]
        private string _trimmedText;

        public void UpdateTrimmedText() => Text = _text;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
