using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public ActionPlayableSettings Settings;

        private Button _button;
        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _text;
        private AnimationCurve _blendCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

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

            Hide();
        }

        public override void EnterClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            Show();
        }

        public override void ProcessFrame(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            if (_button != null)
            {
                if (Settings != null)
                {
                    double startTime = Clip.start;
                    double endTime = Clip.end;
                    double time = Director.time;
                    double startTimeLength = Settings.StartTimeOffset;
                    double endTimeLength = Settings.EndTimeOffset;

                    if (time <= startTime + startTimeLength)
                        _canvasGroup.alpha = Settings.StartFade.Evaluate((float)(time - startTime));
                    else if (time > endTime - endTimeLength)
                        _canvasGroup.alpha = Settings.EndFade.Evaluate((float)(time - endTime + endTimeLength));
                    else
                    {
                        _blendCurve.MoveKey(0, new Keyframe(0, Settings.StartFade.length > 0 ? Settings.StartFade.keys.Last().value : 1));
                        _blendCurve.MoveKey(1, new Keyframe(1, Settings.EndFade.length > 0 ? Settings.EndFade.keys.First().value : 1));

                        _canvasGroup.alpha = _blendCurve.Evaluate(Mathf.Clamp01((float)((time - startTime - startTimeLength) / (endTime - startTime - endTimeLength))));
                    }
                }
                else
                {
                    _canvasGroup.alpha = 1; // Default variant.
                }
            }
        }

        public override void ExitClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            Hide();
        }

        private void Show()
        {
            if (_button != null)
            {
                _canvasGroup.interactable = true;
                _text.text = ChoiceDescription;
            }
        }

        private void Hide()
        {
            if (_button != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _text.text = null;
            }
        }
    }
}
