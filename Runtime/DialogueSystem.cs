using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Dialogue System", menuName = "Dialogue/Dialogue System")]
    public class DialogueSystem : ScriptableObject
    {
        private static List<DialogueSystem> _instances = new List<DialogueSystem>();

        public GameObject Object
        {
            get
            {
                if (_object == null)
                {
                    _object = new GameObject()
                    {
                        name = $"Dialogue System {_instances.IndexOf(this) + 1} (Instanced)",
                    };
                }

                return _object;
            }
        }

        public PlayableDirector Director
        {
            get
            {
                if (_director == null)
                {
                    _director = Object.GetComponent<PlayableDirector>();

                    if (_director == null)   // If not already exist.
                        _director = Object.AddComponent<PlayableDirector>();
                }

                return _director;
            }
        }

        public DialogueSystemBinder Binder
        {
            get
            {
                if (_binder == null)
                {
                    _binder = Object.GetComponent<DialogueSystemBinder>();

                    if (_binder == null)
                        _binder = Director.gameObject.AddComponent<DialogueSystemBinder>();
                }

                return _binder;
            }
        }

        public IReadOnlyDictionary<string, ExposedProperty> ExposedProperties => _exposedProperties;
        public HashSet<ButtonBinder> Buttons => _buttons;
        public Dialogue Dialogue => _dialogue;
        public List<SerializableDictionary<string, ActionPlayableSettings>> ActionOverrideSettings => _actionOverrideSettings;

        [SerializeField] private List<SerializableDictionary<string, ActionPlayableSettings>> _actionOverrideSettings = new();

        [SerializeField, HideInInspector] private GameObject _object;
        [SerializeField, HideInInspector] private PlayableDirector _director;
        [SerializeField, HideInInspector] private DialogueSystemBinder _binder;
        [SerializeField, HideInInspector] private Dialogue _dialogue;
        [SerializeField, HideInInspector] private string _currentInputID;

        private HashSet<ButtonBinder> _buttons = new HashSet<ButtonBinder>();
        private Dictionary<string, ExposedProperty> _exposedProperties;
        private Dictionary<string, UnityAction> _markerEvents = new Dictionary<string, UnityAction>();

        public void GetEvent(string signalName, UnityAction action)
        {
            if (!_markerEvents.ContainsKey(signalName))
                _markerEvents.Add(signalName, delegate { });

            _markerEvents[signalName] += action;
        }

        public void InvokeEvent(string signalName)
        {
            if (!_markerEvents.ContainsKey(signalName))
                _markerEvents.Add(signalName, delegate { });

            _markerEvents[signalName].Invoke();
        }

        public ActionPlayableSettings GetActionSettings(ButtonBinder button, string name)
        {
            int index = _buttons.IndexOf(button);

            if (index != -1 && _actionOverrideSettings.Count > index)
            {
                if (_actionOverrideSettings[index].TryGetValue(name, out var settings))
                    return settings;
            }

            return null;
        }

        /// <summary>
        /// Instatiates a Playable using the provided PlayableAsset and starts playback.
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            if (Director.playableAsset == null)
                return false;

            Director.Play();

            return true;
        }

        /// <summary>
        /// Stops playback of the current Playable and destroys the corresponding graph.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (Director.playableAsset == null)
                return false;

            Director.Stop();

            return true;
        }

        /// <summary>
        /// Pauses playback of the currently running playable.
        /// </summary>
        /// <returns></returns>
        public bool Pause()
        {
            if (Director.playableAsset == null)
                return false;

            Director.Pause();

            return true;
        }

        /// <summary>
        /// Resume playing a paused playable.
        /// </summary>
        /// <returns></returns>
        public bool Resume()
        {
            if (Director.playableAsset == null)
                return false;

            Director.Resume();

            return true;
        }

        public TimelineAsset CreateDialogue(Dialogue dialogue, string inputID)
        {
            _dialogue = dialogue;

            TimelineAsset timeline = DSUtility.CreateDialogue(this, dialogue, inputID);

            // Set exposed properties.
            {
                _exposedProperties = new Dictionary<string, ExposedProperty>();
                foreach (var (name, value) in dialogue.Graph.Properties)
                {
                    var exposedProperty = new ExposedProperty(name, value);

                    if (!dialogue.Graph.PropertyNodes.TryGetValue(name, out List<DSNode> nodesOfProperty))
                        continue;

                    exposedProperty.OnValueChanged += callback =>
                    {
                        foreach (var node in nodesOfProperty)
                        {
                            if (node.TryGetAllProcessors(out var processors))
                            {
                                ValueProcessor processor = (ValueProcessor)processors[0];
                                processor.Value = callback.newValue;
                            }
                        }
                    };

                    _exposedProperties.Add(name, exposedProperty);
                }
            }

            return timeline;
        }

        #region Awake
        private void OnEnable()
        {
            SceneManager.sceneLoaded -= OnLoadScene;
            SceneManager.sceneLoaded += OnLoadScene;

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                Initialize();
#endif
        }

        private void Awake()
        {
#if UNITY_EDITOR
#else
            Initialize();
#endif
        }
        #endregion

        private void Initialize()
        {
            if (_instances.Contains(this))
                return;

            _instances.Add(this);
        }

        private void OnLoadScene(Scene scene, LoadSceneMode mode)
        {

        }

        private void OnDestroy()
        {
            if (_object != null)
                Destroy(_object);
        }
    }
}
