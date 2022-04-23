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
        public Guid GUID { get; }
        public string Name { get; set; }
        public object Value { get; set; }
        public string CustomTypeName { get; }

        public Type PropertyType => Value.GetType();
        public string PropertyTypeName => string.IsNullOrEmpty(CustomTypeName) ? PropertyType.Name : CustomTypeName;

        public VisualElement BuildController();

        internal void Initialize(DGBlackboard blackboard);

    }
}
