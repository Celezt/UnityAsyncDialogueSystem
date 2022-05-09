using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public static class UIElementUtility
    {
        public static VisualElement ControlRow(string labelText, VisualElement control)
        {
            VisualElement rowView = new VisualElement();
            rowView.AddStyleSheet(StyleUtility.STYLE_PATH + "UIElementUtility");
            rowView.AddToClassList("row-view");

            Label label = new Label(labelText);
            label.AddToClassList("row-view-label");
            rowView.Add(label);

            control.AddToClassList("row-view-control");
            rowView.Add(control);

            return rowView;
        }
    }
}
