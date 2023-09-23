using Celezt.Text;
using Celezt.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#nullable enable

namespace Celezt.DialogueSystem
{
    [CreateExtension]
    public class TextExtension : Extension<DialogueAsset>, ITagText, ITime, ITimeIndex
    {
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

        public IReadOnlyList<ITag> TagSequence
        {
            get
            {
                if (_tagSequence == null)
                    UpdateTags();

                return _tagSequence!;
            }
        }

        public int Length => _length;

        public string EditorText
        {
            get => _editorText;
            set
            {
                _editorText = value;
                RefreshText();
            }
        }

        public MutString RuntimeText
        {
            get
            {
                if (_runtimeText is null)
                {
                    _runtimeText = new MutString(_editorText.Length);
                    RefreshText();
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

        double ITime.Time => Asset.Director.time;
        double ITime.StartTime => Asset.StartTime;
        double ITime.EndTime => Asset.EndTime;
        double ITime.FrameRate => ((TimelineAsset)Asset.Director.playableAsset).editorSettings.frameRate;

        [SerializeField, TextArea(10, int.MaxValue)]
        private string _editorText = string.Empty;
        [SerializeField, Min(0)]
        private float _startOffset;
        [SerializeField, Min(0)]
        private float _endOffset;
        [SerializeField]
        private AnimationCurve _editorVisibilityCurve = AnimationCurve.Linear(0, 0, 1, 1);

#if UNITY_EDITOR
        private bool _hasUpdated;
#endif
        private int _length;
        private MutString? _runtimeText;
        private AnimationCurve? _runtimeVisibilityCurve;
        private List<ITag>? _tagSequence;

        private float _previousValue = float.MaxValue;

        public void RefreshText()
        {
            RuntimeVisibilityCurve.keys = EditorVisibilityCurve.keys;

            Span<char> span = stackalloc char[_editorText.Length];
            _editorText.AsSpan().CopyTo(span);

            _tagSequence = Tags.GetTagSequence(span, this);
            _length = Tags.GetTextLength(span);

            _runtimeText?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));
            Tags.InvokeAll(_tagSequence);
#if UNITY_EDITOR
            EditorUtility.IsDirty(Asset);
            _hasUpdated = true;
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

        protected override void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            if (!RuntimeText.IsEmpty)
                clip.displayName = Tags.TrimTextTags(RuntimeText.ReadOnlySpan);

#if UNITY_EDITOR
            if (_hasUpdated)
                _hasUpdated = false;
            else
#endif
                UpdateTags();
        }

        protected override void OnEnter(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData)
        {
            float currentValue = ((ITime)this).VisibilityInterval * Length;

            foreach (ITag tag in TagSequence)
            {
                if (tag is ITagSpan tagSpan)
                {
                    var range = tagSpan.Range;

                    if (StartOffset == 0 && range.start == 0 && currentValue < 1 ||
                        EndOffset == 0 && range.end == Length && currentValue > Length - 1)
                        tagSpan.OnEnter();
                }
                else if (tag is ITagSingle tagSingle)
                {
                    int index = tagSingle.Index;

                    if (StartOffset == 0 && index == 0 && currentValue < 1 ||
                        EndOffset == 0 && index == Length && currentValue > Length - 1)
                        tagSingle.OnInvoke();
                }
            }
        }

        protected override void OnProcess(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData)
        {
            float currentValue = ((ITime)this).VisibilityInterval * Length;

            foreach (ITag tag in TagSequence)
            {
                if (tag is ITagSpan tagSpan)
                {
                    var range = tagSpan.Range;

                    if (mixer.IsPlayingForward ?
                        (range.start == 0 && StartOffset > 0 ?
                            _previousValue <= range.start && currentValue > range.start :
                            _previousValue < range.start && currentValue >= range.start) :
                        (range.end == 0 && StartOffset > 0 ?
                            _previousValue > range.end && currentValue <= range.end :
                            _previousValue >= range.end && currentValue < range.end))
                        tagSpan.OnEnter();
                    else if (mixer.IsPlayingForward ?
                        (range.end == 0 && StartOffset > 0 ?
                            _previousValue <= range.end && currentValue > range.end :
                            _previousValue < range.end && currentValue >= range.end) :
                        (range.start == 0 && StartOffset > 0 ?
                            _previousValue > range.start && currentValue <= range.start :
                            _previousValue >= range.start && currentValue < range.start))
                        tagSpan.OnExit();
                    else if (currentValue > range.start && currentValue < range.end)
                        tagSpan.OnProcess(Mathf.RoundToInt(currentValue));
                }
                else if (tag is ITagSingle tagSingle)
                {
                    int index = tagSingle.Index;

                    if (mixer.IsPlayingForward ?
                    (index == 0 && StartOffset > 0 ?
                        _previousValue <= index && currentValue > index :
                        _previousValue < index && currentValue >= index) :
                    (index == 0 && StartOffset > 0 ?
                        _previousValue > index && currentValue <= index :
                        _previousValue >= index && currentValue < index))
                        tagSingle.OnInvoke();
                }
            }

            _previousValue = currentValue;
        }

        protected override void OnExit(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData)
        {
            float currentValue = ((ITime)this).VisibilityInterval * Length;

            foreach (ITag tag in TagSequence)
            {
                if (tag is ITagSpan tagSpan)
                {
                    var range = tagSpan.Range;

                    if (EndOffset == 0 && range.end == Length && currentValue > Length - 1 ||
                        StartOffset == 0 && range.start == 0 && currentValue < 1)
                        tagSpan.OnExit();
                }
                else if (tag is ITagSingle tagSingle)
                {
                    int index = tagSingle.Index;

                    if (EndOffset == 0 && index == Length && currentValue > Length - 1 ||
                        StartOffset == 0 && index == 0 && currentValue < 1)
                        tagSingle.OnInvoke();
                }
            }
        }
    }
}
