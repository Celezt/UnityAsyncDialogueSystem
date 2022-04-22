using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Celezt.DialogueSystem.Editor.Utilities;
using UnityEditor.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class BlackboardFieldPropertyView : VisualElement
    {
        private IBlackboardProperty _property;

        public BlackboardFieldPropertyView(IBlackboardProperty property)
        {
            _property = property;

            this.AddStyleSheet(StyleUtility.STYLE_PATH + "DGBlackboard");

            AddRow("Value", property.BuildController());
        }

        private VisualElement AddRow(string labelText, VisualElement control)
        {
            VisualElement rowView = new VisualElement();
            rowView.AddToClassList("row-view");

            Label label = new Label(labelText);
            label.AddToClassList("row-view-label");
            rowView.Add(label);

            control.AddToClassList("row-view-control");
            rowView.Add(control);
            Add(rowView);

            return rowView;
        }
    }
}
