using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class BlackboardProperty<TValue, TPort> : IBlackboardProperty where TPort : IPortType 
    {
        public Guid GUID => _guid;
        public Type PortType => typeof(TPort);
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    if (string.IsNullOrEmpty(CustomTypeName))
                        _name = "New " + typeof(TValue).Name;
                    else
                        _name = "New " + CustomTypeName;
                }

                return _name;
            }
            set => _name = value;
        }
        public object Value
        {
            get => _value;
            set => _value = (TValue)value;
        }
        public bool hasUnsavedChanges
        {
            get => _blackboard.graphView.EditorWindow.hasUnsavedChanges;
            set => _blackboard.graphView.EditorWindow.hasUnsavedChanges = value;
        }
        public virtual string CustomTypeName { get; } = null;

        protected readonly Guid _guid = Guid.NewGuid();

        protected TValue _value;
        protected string _name;
        protected DGBlackboard _blackboard;

        public abstract VisualElement BuildController();

        public bool Equals(IBlackboardProperty other) => other.GUID == _guid;
        public override bool Equals(object obj)
        {
            if (obj is IBlackboardProperty other)
                return Equals(other);
            else if (obj is Guid guid)
                return _guid == guid;

            return false;
        }
        public override int GetHashCode() => _guid.GetHashCode();
        public override string ToString() => _name;

        void IBlackboardProperty.Initialize(DGBlackboard blackboard)
        {
            _blackboard = blackboard;
        }
    }
}
