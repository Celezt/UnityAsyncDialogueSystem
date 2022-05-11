using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Celezt.DialogueSystem
{
    [RequireComponent(typeof(Button))]
    public class ButtonBinder : MonoBehaviour
    {
        public bool IsBinded => Button != null;
        public bool IsBorrowed => _owner != null;
        public Button Button
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }

                return _button;
            }
        }
        public UnityEngine.Object Owner => _owner;

        private Button _button;
        private UnityEngine.Object _owner;

        /// <summary>
        /// Borrow binder as the owner.
        /// </summary>
        /// <param name="owner">Unity Object owner.</param>
        /// <returns>If borrowable or already owned by the owner.</returns>
        public bool Borrow(UnityEngine.Object owner)
        {
            if (_owner != null && owner == _owner)
                return true;

            if (IsBorrowed)
                return false;

            _owner = owner;

            return true;
        }

        /// <summary>
        /// Release ownership if it is borrowed.
        /// </summary>
        /// <param name="owner">Unity Object owner.</param>
        /// <returns>If the same owner.</returns>
        public bool Released(UnityEngine.Object owner)
        {
            if (_owner != null && _owner == owner)
            {
                _owner = null;
                return true;
            }

            return false;
        }
    }
}
