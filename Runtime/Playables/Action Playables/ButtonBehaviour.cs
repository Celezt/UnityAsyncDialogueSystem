using Celezt.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ButtonBehaviour : EPlayableBehaviour
    {
        private AnimationCurve _blendCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

        [SerializeField, HideInInspector]
        private Ref<bool> _isActive = new Ref<bool>();
        private Ref<ButtonBinder> _buttonBinder = new Ref<ButtonBinder>();
        private Ref<CanvasGroup> _canvasGroup = new Ref<CanvasGroup>();
        private Ref<TextMeshProUGUI> _textMesh = new Ref<TextMeshProUGUI>();

        public void Hide()
        {
            if (_buttonBinder.Value != null)
            {
                _canvasGroup.Value.alpha = 0;
                _canvasGroup.Value.interactable = false;
            }
        }

        public void Show()
        {
            ButtonAsset asset = Asset as ButtonAsset;

            if (_buttonBinder.Value == null)
                BindButton();

            if (_buttonBinder.Value != null)
            {
                ProcessVisibility();
                _canvasGroup.Value.interactable = true;
                if (_textMesh.Value != null)
                    _textMesh.Value.text = asset.Text;
            }
        }

        public override void OnCreate()
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
            _buttonBinder.Value = asset.ButtonReference.Resolve(graph.GetResolver());

            if (_buttonBinder.Value != null)
            {
                _canvasGroup.Value = _buttonBinder.Value.GetComponentInChildren<CanvasGroup>();
                _textMesh.Value = _buttonBinder.Value.GetComponentInChildren<TextMeshProUGUI>();
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

        public override void OnEnter(Playable playable, FrameData info, float weight, object playerData)
        {
            ButtonAsset asset = Asset as ButtonAsset;

            BindButton();

            if (_buttonBinder.Value != null)
            {
                _canvasGroup.Value.interactable = true;
                if (_textMesh.Value != null)
                    _textMesh.Value.text = asset.Text;
            }
        }

        public override void OnProcess(Playable playable, FrameData info, float weight, object playerData)
        {
            ProcessVisibility();
        }

        public override void OnExited(Playable playable, FrameData info, float weight, object playerData)
        {
            Hide();
            BindButton();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            ButtonAsset asset = Asset as ButtonAsset;

            Hide();

            _buttonBinder.Value?.Released(asset);
        }

        private void ProcessVisibility()
        {
            ButtonAsset asset = Asset as ButtonAsset;

            if (_buttonBinder.Value != null && _isActive.Value)
            {
                if (asset.Settings != null)
                {
                    double startTime = Clip.start;
                    double endTime = Clip.end;
                    double time = Director.time;
                    double startTimeLength = asset.Settings.StartTimeOffset;
                    double endTimeLength = asset.Settings.EndTimeOffset;

                    if (time <= startTime + startTimeLength)
                        _canvasGroup.Value.alpha = asset.Settings.StartFade.Evaluate((float)(time - startTime));
                    else if (time > endTime - endTimeLength)
                        _canvasGroup.Value.alpha = asset.Settings.EndFade.Evaluate((float)(time - endTime + endTimeLength));
                    else
                    {
                        _blendCurve.MoveKey(0, new Keyframe(0, asset.Settings.StartFade.length > 0 ? asset.Settings.StartFade.keys.Last().value : 1));
                        _blendCurve.MoveKey(1, new Keyframe(1, asset.Settings.EndFade.length > 0 ? asset.Settings.EndFade.keys.First().value : 1));
                        // Blend the first and last point in start and end curve.
                        _canvasGroup.Value.alpha = _blendCurve.Evaluate(Mathf.Clamp01((float)((time - startTime - startTimeLength) / (endTime - startTime - endTimeLength))));
                    }
                }
                else
                {
                    _canvasGroup.Value.alpha = 1; // Default variant.
                }
            }
        }

        private void BindButton()
        {
            ButtonAsset asset = Asset as ButtonAsset;

            if (!_isActive.Value)
                return;

            if (asset.System != null)
            {
                if (_buttonBinder.Value?.Released(asset) ?? false)
                    return;

                ButtonBinder oldButton = _buttonBinder;  
                _buttonBinder.Value = asset.System.Buttons.FirstOrDefault(x => x.Borrow(asset));

                if (_buttonBinder.Value != null && _buttonBinder.Value != oldButton)
                {
                    asset.ButtonReference.exposedName = Guid.NewGuid().ToString();
                    Director.SetReferenceValue(asset.ButtonReference.exposedName, _buttonBinder);
                    asset.Settings = asset.System.GetActionSettings(_buttonBinder, asset.OverrideSettingName);

                    if (asset.OnClick != null)
                        _buttonBinder.Value.Button.onClick.AddListener(asset.OnClick);

                    _canvasGroup.Value = _buttonBinder.Value.GetComponentInChildren<CanvasGroup>();
                    _textMesh.Value = _buttonBinder.Value.GetComponentInChildren<TextMeshProUGUI>();
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
