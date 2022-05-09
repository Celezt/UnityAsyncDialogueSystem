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

        [SerializeField]
        private Dialogue _dialogue;


        private GameObject _object;
        private PlayableDirector _director;
        private DialogueSystemBinder _binder;

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
            if (Binder != null)
            {
                DSNode inputNode = _dialogue.Graph.GetInputNode("ID");
                TimelineAsset timeline = DSUtility.CreateTimeline(this, inputNode);
            }
        }

        private void OnDestroy()
        {
            Destroy(_object);
        }
    }
}
