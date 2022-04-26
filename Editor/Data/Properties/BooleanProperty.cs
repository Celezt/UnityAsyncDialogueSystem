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
        private enum ConditionState
        {
            False,
            True,
        }

        public override VisualElement BuildController()
        {
            var control = new EnumField(ConditionState.False)
            {
                value = ToEnum(_value),
            };
            control.RegisterValueChangedCallback(x =>
            {
                Value = ToBool((ConditionState)x.newValue);
                hasUnsavedChanges = true;
            });
            return control;
        }

        private static ConditionState ToEnum(bool value) => value switch
        {
            false => ConditionState.False,
            true => ConditionState.True,
        };

        private static bool ToBool(ConditionState value) => value switch
        {
            ConditionState.True => true,
            _ => false,           
        };
    }
}
