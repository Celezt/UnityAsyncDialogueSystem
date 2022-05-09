using System;
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
    public class ButtonBehaviour : DSPlayableBehaviour
    {
        private Button _button;
        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _textMesh;
        private AnimationCurve _blendCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

        [SerializeField, HideInInspector]
        private Ref<bool> _isActive = new Ref<bool>();

        public void Hide()
        {
            if (_button != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
            }
        }

        public void Show()
        {
            if (_button != null)
            {
                ProcessVisibility();
                _canvasGroup.interactable = true;
            }
        }

        public override void OnCreateClip()
        {
            ButtonAsset asset = Asset as ButtonAsset;
            if (asset.Settings == null)   // Get previous clip's setting if it exist.
            {
                ButtonAsset previousAsset = null;
                foreach (var clip in Clip.GetParentTrack().GetClips())
                {
                    if (clip == Clip)
                    {
                        if (previousAsset != null)
                        {
#if UNITY_EDITOR
                            UnityEditor.EditorGUI.BeginChangeCheck();
#endif
                            asset.Settings = previousAsset.Settings;
#if UNITY_EDITOR
                            UnityEditor.EditorGUI.EndChangeCheck();
#endif
                            break;
                        }
                    }
                    else
                    {
                        if (clip.asset is ButtonAsset clipAsset)    // The last asset of correct time.
                            previousAsset = clipAsset;
                    }
                }
            }
        }

        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            ButtonAsset asset = Asset as ButtonAsset;
            _button = asset.ButtonReference.Resolve(graph.GetResolver());

            if (_button != null)
            {
                _canvasGroup = _button.GetComponentInChildren<CanvasGroup>();
                _textMesh = _button.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (!string.IsNullOrWhiteSpace(asset.Text))
                clip.displayName = asset.Text;

            Hide();

            if (asset.Condition != null)
            {
                asset.Condition.InitializeTree();
                asset.Condition.OnChanged.RemoveListener(ConditionChanges);
                asset.Condition.OnChanged.AddListener(ConditionChanges);
                ConditionChanges();
            }
            else
                _isActive.Value = true;
        }

        public override void EnterClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            ButtonAsset asset = Asset as ButtonAsset;

            if (_button != null)
            {
                _canvasGroup.interactable = true;
                if (_textMesh != null)
                    _textMesh.text = asset.Text;
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            ProcessVisibility();
        }

        public override void ExitClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            Hide();
        }

        private void ProcessVisibility()
        {
            ButtonAsset asset = Asset as ButtonAsset;

            if (_button != null && _isActive.Value)
            {
                if (asset.Settings != null)
                {
                    double startTime = Clip.start;
                    double endTime = Clip.end;
                    double time = Director.time;
                    double startTimeLength = asset.Settings.StartTimeOffset;
                    double endTimeLength = asset.Settings.EndTimeOffset;

                    if (time <= startTime + startTimeLength)
                        _canvasGroup.alpha = asset.Settings.StartFade.Evaluate((float)(time - startTime));
                    else if (time > endTime - endTimeLength)
                        _canvasGroup.alpha = asset.Settings.EndFade.Evaluate((float)(time - endTime + endTimeLength));
                    else
                    {
                        _blendCurve.MoveKey(0, new Keyframe(0, asset.Settings.StartFade.length > 0 ? asset.Settings.StartFade.keys.Last().value : 1));
                        _blendCurve.MoveKey(1, new Keyframe(1, asset.Settings.EndFade.length > 0 ? asset.Settings.EndFade.keys.First().value : 1));
                        // Blend the first and last point in start and end curve.
                        _canvasGroup.alpha = _blendCurve.Evaluate(Mathf.Clamp01((float)((time - startTime - startTimeLength) / (endTime - startTime - endTimeLength))));
                    }
                }
                else
                {
                    _canvasGroup.alpha = 1; // Default variant.
                }
            }
        }

        private void ConditionChanges()
        {
            ButtonAsset asset = Asset as ButtonAsset;

            _isActive.Value =  Convert.ToBoolean(asset.Condition.GetValue(0));

            if (_isActive == false)
                Hide();
            else
                Show();
        }
    }
}
