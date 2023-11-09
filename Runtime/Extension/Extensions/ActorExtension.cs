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
        private string _editorText = string.Empty;

#if UNITY_EDITOR
        private bool _hasUpdated;
#endif
        private int _length;
        private MutString? _runtimeText;
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

        public void RefreshText()
        {
            if (Asset == null)
                return;

            Span<char> span = stackalloc char[_editorText.Length];
            _editorText.AsSpan().CopyTo(span);

            _tagSequence = Tags.GetTagSequence(span, this);
            _length = Tags.GetTextLength(span);

            _runtimeText?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));
            Tags.InvokeAll(_tagSequence);
#if UNITY_EDITOR
            _hasUpdated = true;
#endif
        }

        public void UpdateTags()
        {
            if (Asset == null)
                return;

            Span<char> span = stackalloc char[_editorText.Length];
            _editorText.AsSpan().CopyTo(span);
            _runtimeText?.Set(Tags.TrimTextTags(span, Tags.TagVariation.Custom));

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
