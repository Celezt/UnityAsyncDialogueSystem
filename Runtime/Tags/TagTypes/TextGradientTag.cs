using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class TextGradientTag : TagSpan<DialogueAsset>
    {
        [Implicit]
        public string Color { get; set; }

        public override void OnCreate(RangeInt range, DialogueAsset binder)
        {
            if (string.IsNullOrWhiteSpace(Color))
                return;

            var span = Color.AsSpan();
            int index = span.IndexOf(" to ", StringComparison.OrdinalIgnoreCase);

            ReadOnlySpan<char> firstColorSpan = index > 0 ? span.Slice(0, index + 1).Trim() : span;
            ReadOnlySpan<char> secondColorSpan = index > 0 ? span.Slice(index + 4).Trim() : default;
            UnityEngine.Color firstColor = UnityEngine.Color.white;
            UnityEngine.Color secondColor = UnityEngine.Color.white;

            if (!ColorUtility.TryParseHtmlString(firstColorSpan.ToString().ToLowerInvariant(), out firstColor))
                return;

            secondColor = firstColor;

            if (index > 0 && !ColorUtility.TryParseHtmlString(secondColorSpan.ToString().ToLowerInvariant(), out secondColor))
                return;

            for (int i = range.start; i < range.length; i++)
            {
                float t = i / (float)range.length;
                float r = Mathf.Lerp(firstColor.r, secondColor.r, t);
                float g = Mathf.Lerp(firstColor.g, secondColor.g, t);
                float b = Mathf.Lerp(firstColor.b, secondColor.b, t);
                float a = Mathf.Lerp(firstColor.a, secondColor.a, t);
                var color = new UnityEngine.Color(r, g, b, a);

                binder.TryInsertBefore(i, $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>");
                binder.TryInsertAfter(i, "</color>");
            }
        }
    }
}
