using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Celezt.DialogueSystem
{
    public class DialogueAsset : DSPlayableAsset, ITime
    {
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                    TrimmedText = value;

                _text = value;
            }
        }

        public string Actor
        {
            get => _actor;
            set => _actor = value;
        }

        public string TrimmedText
        {
            get
            {
                if (_trimmedText == null)
                    UpdateTrimmedText();

                return _trimmedText;
            }
            private set
            {
                _trimmedText = Tags.TrimTextTags(value);
#if UNITY_EDITOR
                EditorUtility.IsDirty(this);
#endif
            }
        }

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

        public void UpdateTrimmedText() => TrimmedText = _text;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
