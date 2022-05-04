using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Celezt.DialogueSystem
{
    public class ButtonChoiceBehaviour : DSPlayableBehaviour
    {
        public ExposedReference<Button> ButtonReference;
        public string ChoiceDescription;

        private Button _button;
        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _text;

        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount, TimelineClip clip)
        {
            _button = ButtonReference.Resolve(graph.GetResolver());

            if (_button != null)
            {
                _canvasGroup = _button.GetComponentInChildren<CanvasGroup>();
                _text = _button.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (!string.IsNullOrWhiteSpace(ChoiceDescription))
                clip.displayName = ChoiceDescription;
        }

        public override void EnterClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            if (_button != null)
            {
                _canvasGroup.alpha = 1f;
                _text.text = ChoiceDescription;
            }
        }

        public override void ExitClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            if (_button != null)
            {
                _canvasGroup.alpha = 0f;
                _text.text = null;
            }
        }
    }
}
