using System;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    /// <summary>
    /// Thanks to https://forum.unity.com/threads/4-6-editorapplication-modifierkeyschanged-how-to-find-out-which-key-was-pressed.357367/#post-2705846
    /// </summary>
    [InitializeOnLoad]
    public static class GlobalEventHandler
    {
        public static event Action<Event> OnEvent;
        public static Event CurrentEvent;
        public static bool RegistrationSucceeded = false;

        static GlobalEventHandler()
        {
            RegistrationSucceeded = false;
            string msg = "";
            try
            {
                System.Reflection.FieldInfo info = typeof(EditorApplication).GetField(
                    "globalEventHandler",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                    );
                if (info != null)
                {
                    EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

                    value -= OnChange;
                    value += OnChange;

                    info.SetValue(null, value);

                    RegistrationSucceeded = true;
                }
                else
                {
                    msg = "globalEventHandler not found";
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            finally
            {
                if (!RegistrationSucceeded)
                {
                    Debug.LogWarning("GlobalEventHandler: error while registering for globalEventHandler: " + msg);
                }
            }
        }

        private static void OnChange()
        {
            OnEvent?.Invoke(Event.current);
            CurrentEvent = Event.current;
        }
    }
}
