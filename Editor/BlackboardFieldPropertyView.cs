using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class BlackboardFieldPropertyView : VisualElement
    {
        private IBlackboardProperty _property;

        public BlackboardFieldPropertyView(IBlackboardProperty property)
        {
            _property = property;

            Add(UIElementUtility.ControlRow("Value", _property.BuildController()));
        }
    }
}
