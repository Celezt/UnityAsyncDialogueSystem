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

            var span = Color.AsSpan().Trim();

            Span<Color> colors = stackalloc Color[16];
            int count = 0;

            ReadOnlySpan<char> colorsSpan = span;
            UnityEngine.Color currentColor = UnityEngine.Color.white;
            while (count < colors.Length && !colorsSpan.IsEmpty)
            {
                int whiteSpaceIndex = colorsSpan.IndexOf(' ');

                ReadOnlySpan<char> nextColorSpan = whiteSpaceIndex > 0 ? colorsSpan.Slice(0, whiteSpaceIndex) : colorsSpan;
                colorsSpan = whiteSpaceIndex > 0 ? colorsSpan.Slice(whiteSpaceIndex + 1).TrimStart() : default;

                if (nextColorSpan.IsNumbers())
                {
                    int number = Mathf.Min(Mathf.Abs(int.Parse(nextColorSpan)), colors.Length - count - 1);

                    while(--number > 0)
                        colors[count++] = currentColor;
                }

                if (nextColorSpan.Equals("to", StringComparison.OrdinalIgnoreCase)) // Ignore if it is a to.
                    continue;

                if (!ColorUtility.TryParseHtmlString(nextColorSpan.ToString().ToLowerInvariant(), out currentColor))
                    continue;

                colors[count++] = currentColor;
            }

            colors.Slice(count).Fill(count > 0 ? colors[count - 1] : UnityEngine.Color.white);

            if (count == 0)
                return;

            for (int i = 0; i < range.length; i++)
            {
                float t = (i / (float)range.length) * (count - 1);

                int colorIndex = Mathf.FloorToInt(t);
                var firstColor = colors[colorIndex];
                var secondColor = colors[colorIndex + 1];

                var color = UnityEngine.Color.Lerp(firstColor, secondColor, t % 1);

                binder.TryInsertBefore(range.start + i, $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>", isWhitespaceAllowed: false);
                binder.TryInsertAfter(range.start + i, "</color>", isWhitespaceAllowed: false);
            }

        }
    }
}
