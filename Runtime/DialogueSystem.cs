using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Dialogue System", menuName = "Dialogue/Dialogue System")]
    public class DialogueSystem : ScriptableObject
    {
        private static List<DialogueSystem> _instances = new List<DialogueSystem>();

        public PlayableDirector Director
        {
            get
            {
                if (_director == null)
                    _director = CreateDirector();

                return _director;
            }
        }

        private PlayableDirector _director;

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

        private PlayableDirector CreateDirector()
        {
            GameObject gameObject = new GameObject()
            {
                name = $"Dialogue System {_instances.IndexOf(this) + 1} (Instanced)",
            };

            return gameObject.AddComponent<PlayableDirector>();
        }
    }
}
