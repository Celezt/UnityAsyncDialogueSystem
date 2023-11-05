using Celezt.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#nullable enable

namespace Celezt.DialogueSystem
{
    [CreateExtension]
    public class ActorExtension : Extension<DialogueAsset>, ITagText
    {
        [SerializeField]
        private string _editorActor = string.Empty;

#if UNITY_EDITOR
        private bool _hasUpdated;
#endif
        private int _length;
        private MutString? _runtimeActor;
        private List<ITag>? _tagSequence;

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
            get => _editorActor;
            set
            {
                _editorActor = value;
                RefreshText();
            }
        }

        public MutString RuntimeText
        {
            get
            {
                if (_runtimeActor is null)
                {
                    _runtimeActor = new MutString(_editorActor.Length);
                    RefreshText();
                }

                return _runtimeActor;
            }
        }

        public void RefreshText()
        {
            Span<char> span = stackalloc char[_editorActor.Length];
            _editorActor.AsSpan().CopyTo(span);

            _tagSequence = Tags.GetTagSequence(span, this);
            _length = Tags.GetTextLength(span);

            _runtimeActor?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));
            Tags.InvokeAll(_tagSequence);
#if UNITY_EDITOR
            _hasUpdated = true;
#endif
        }

        public void UpdateTags()
        {
            Span<char> span = stackalloc char[_editorActor.Length];
            _editorActor.AsSpan().CopyTo(span);
            _runtimeActor?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));

            _tagSequence ??= Tags.GetTagSequence(EditorText, this);

            Tags.InvokeAll(_tagSequence);
        }

        protected override void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
#if UNITY_EDITOR
            if (_hasUpdated)
                _hasUpdated = false;
            else
#endif
                UpdateTags();
        }
    }
}
