using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [BlackboardProperty]
    public class FloatProperty : BlackboardProperty<float>
    {
        public override string CustomTypeName => "float";

        public override VisualElement BuildController()
        {
            var control = new FloatField
            {
                value = _value,           
            };
           
            return control;
        }
    }
}
