using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class BlackboardProperty<TValue, TPort> : IBlackboardProperty where TPort : IPortType
    {
        public Guid ID => _id;
        public Type PortType => typeof(TPort);
        public string ValueTypeName => _blackboard.GetValueName(typeof(BlackboardProperty<TValue, TPort>));
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = "New " + _blackboard.GetValueName(typeof(BlackboardProperty<TValue, TPort>));

                return _name;
            }
            set
            {
                _onNameChangedCallback.Invoke(ChangeEvent<string>.GetPooled(_name, value));
                _name = value;
            }
        }
        public object Value
        {
            get => _value;
            set
            {
                _onValueChangedCallback.Invoke(ChangeEvent<object>.GetPooled(_value, value));
                _value = (TValue)value;
            }
        }
        public bool hasUnsavedChanges
        {
            get => _blackboard.graphView.EditorWindow.hasUnsavedChanges;
            set => _blackboard.graphView.EditorWindow.hasUnsavedChanges = value;
        }


        public event Action OnDestroyCallback = delegate { };

        protected TValue _value;
        protected string _name;
        protected DGBlackboard _blackboard;

        private event EventCallback<ChangeEvent<object>> _onValueChangedCallback = delegate { };
        private event EventCallback<ChangeEvent<string>> _onNameChangedCallback = delegate { };

        private Guid _id = Guid.NewGuid();

        public abstract VisualElement BuildController();
        public virtual void OnDestroy() { }

        public bool Equals(IBlackboardProperty other) => other.ID == _id;
        public override bool Equals(object obj)
        {
            if (obj is IBlackboardProperty other)
                return Equals(other);
            else if (obj is Guid guid)
                return _id == guid;

            return false;
        }
        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => _name;

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<object>> callback)
        {
            _onValueChangedCallback += callback;
        }

        public void RegisterNameChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            _onNameChangedCallback += callback;
        }

        void IBlackboardProperty.Initialize(DGBlackboard blackboard)
        {
            _blackboard = blackboard;
        }

        void IBlackboardProperty.OnDestroy()
        {
            OnDestroyCallback.Invoke();
        }

        void IBlackboardProperty.SetID(Guid id)
        {
            _id = id;
        }
    }
}
