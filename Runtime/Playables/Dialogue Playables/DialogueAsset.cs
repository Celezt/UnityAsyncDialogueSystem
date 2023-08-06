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
                    _tagSequence = Tags.GetTagSequence(RawText, this);

                return _tagSequence;
            }
        }

        public double StartTime => Clip.start;
        public double EndTime => Clip.end;
        public float TimeLengthUnscaled => (float)(EndTime - StartTime);
        public float TimeLength => TimeLengthUnscaled - EndOffset - StartOffset;

        /// <summary>
        /// How much time is left in unit interval [0-1]. Unaffected by speed.
        /// </summary>
        public float IntervalUnscaled =>
            Mathf.Clamp01((float)((Director.time - StartTime) / TimeLengthUnscaled));

        /// <summary>
        /// How much time is left dependent on speed in unit interval [0-1]. 0 if before and 1 if after.
        /// </summary>
        public float Interval =>
            RuntimeVisibilityCurve.Evaluate(Mathf.Clamp01((float)((Director.time - StartTime - StartOffset) / TimeLength)));

        public AnimationCurve VisibilityCurve => _visibilityCurve;

        public AnimationCurve RuntimeVisibilityCurve
        {
            get
            {
                if (_runtimeVisibilityCurve == null)
                    _runtimeVisibilityCurve = new AnimationCurve(_visibilityCurve.keys);

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

                _startOffset = Mathf.Clamp(value, 0, TimeLengthUnscaled - _endOffset);

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

                _endOffset = Mathf.Clamp(value, 0, TimeLengthUnscaled - _startOffset);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
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

        private float _lastUpdated;

        private AnimationCurve? _runtimeVisibilityCurve;
        private List<ITag>? _tagSequence;

        public void RefreshDialogue()
        {
            if (HasUpdated())
                return;

            _trimmedText = Tags.TrimTextTags(_text, Tags.TagVariation.Custom);
            _length = Tags.GetTextLength(_trimmedText);
            _tagSequence = Tags.GetTagSequence(RawText, this);
#if UNITY_EDITOR
            EditorUtility.IsDirty(this);
#endif
        }

        public void UpdateTags()
        {
            if (HasUpdated())
                return;

            foreach (ITag tag in TagSequence)
                tag.OnCreate();

            Tags.InvokeSystems(TagSequence, this);
        }

        public int GetIndexByTime(double time)
        {
            float interval = RuntimeVisibilityCurve.Evaluate(Mathf.Clamp01((float)((time - StartTime + StartOffset) / (EndTime - EndOffset - StartTime))));
            return Mathf.CeilToInt(interval * Tags.GetTextLength(Text));
        }


        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }

        private bool HasUpdated()
        {
            float time = Time.unscaledDeltaTime;

            if (_lastUpdated == time)
                return true;

            _lastUpdated = time;

            return false;
        }
    }
}
