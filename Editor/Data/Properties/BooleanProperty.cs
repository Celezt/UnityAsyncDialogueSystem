using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [BlackboardProperty("bool")]
    public class BooleanProperty : BlackboardProperty<bool, ConditionPortType>
    {
        public override VisualElement BuildController()
        {
            var control = new Toggle
            {
                value = _value,
            };
            control.RegisterValueChangedCallback(x =>
            {
                Value =  x.newValue;
                hasUnsavedChanges = true;
            });

            return control;
        }
    }
}
