using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public interface IBlackboardProperty : IEquatable<IBlackboardProperty>
    {
        public Guid ID { get; }
        public Type PortType { get; }
        public string Name { get; set; }
        public object Value { get; set; }
        public string ValueTypeName { get; }

        public Type ValueType => Value.GetType();

        public event Action OnDestroyCallback;

        public VisualElement BuildController();
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<object>> callback);
        public void RegisterNameChangedCallback(EventCallback<ChangeEvent<string>> callback);

        internal void Initialize(DGBlackboard blackboard);
        internal void OnDestroy();
        internal void SetID(Guid id);


    }
}
