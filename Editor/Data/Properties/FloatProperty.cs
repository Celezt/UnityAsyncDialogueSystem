using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [BlackboardProperty("float")]
    public class FloatProperty : BlackboardProperty<float, NumericPortType>
    {
        public override VisualElement BuildController()
        {
            var control = new FloatField
            {
                value = _value,           
            };
            control.RegisterValueChangedCallback(x =>
            {
                Value = x.newValue;
                hasUnsavedChanges = true;
                
            });
            return control;
        }
    }
}
