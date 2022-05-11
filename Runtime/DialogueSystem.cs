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

        public Dialogue CurrentDialogue => _currentDialogue;
        public List<ActionPlayableSettings> ActionOverrideSettings => _actionOverrideSettings;


        /// <summary>
        /// Trail of the current dialogue graph.
        /// </summary>
        internal List<DSNode> _previousNodes = new List<DSNode>();

        [SerializeField]
        private List<ActionPlayableSettings> _actionOverrideSettings = new List<ActionPlayableSettings>();
        [SerializeField, HideInInspector]
        private GameObject _object;
        [SerializeField, HideInInspector]
        private PlayableDirector _director;
        [SerializeField, HideInInspector]
        private DialogueSystemBinder _binder;
        [SerializeField, HideInInspector]
        private Dialogue _currentDialogue;
        [SerializeField, HideInInspector]
        private string _currentInputID;

        private HashSet<ButtonBinder> _buttons = new HashSet<ButtonBinder>();

        public bool AddButtonRange(IEnumerable<ButtonBinder> buttons)
        {
            bool isAnyExisting = false;
            foreach (var button in buttons)
                isAnyExisting |= !AddButton(button);

            return isAnyExisting;
        }

        public bool AddButton(ButtonBinder button)
        {
            return _buttons.Add(button);
        }

        public ButtonBinder BorrowFirstOrDefaultButton(UnityEngine.Object owner) 
        {
            return _buttons.FirstOrDefault(x => x.Borrow(owner));
        }

        public ActionPlayableSettings GetActionSettings(ButtonBinder button)
        {
            int index = _buttons.IndexOf(button);

            if (index != -1 && _actionOverrideSettings.Count > index)
                return _actionOverrideSettings[index];

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
            _previousNodes.Clear();
            _currentDialogue = dialogue;

            TimelineAsset timeline = DSUtility.CreateDialogue(this, dialogue, inputID);

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
