using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [Serializable]
    public struct SerializedVector2
    {
        public float x;
        public float y;

        public SerializedVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(SerializedVector2 v) => new Vector2(v.x, v.y);
        public static implicit operator SerializedVector2(Vector2 v) => new SerializedVector2(x: v.x, y: v.y);
    }
}
