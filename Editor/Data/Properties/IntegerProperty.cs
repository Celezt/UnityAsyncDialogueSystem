using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [BlackboardProperty]
    public class IntegerProperty : BlackboardProperty<int>
    {
        public override string CustomTypeName => "int";

        public override VisualElement BuildController()
        {
            var control = new IntegerField
            {
                value = _value,           
            };
            control.RegisterValueChangedCallback(x =>
            {
                _value = x.newValue;
                hasUnsavedChanges = true;

            });
            return control;
        }
    }
}
