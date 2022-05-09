using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

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

        internal List<DSNode> _previousNodes = new List<DSNode>();

        public Dialogue CurrentDialogue => _currentDialogue;

        private GameObject _object;
        private PlayableDirector _director;
        private DialogueSystemBinder _binder;
        private Dialogue _currentDialogue;

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

            Director.playableAsset = timeline;

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
