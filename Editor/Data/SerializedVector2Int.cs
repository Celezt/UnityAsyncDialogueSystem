using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [Serializable]
    public struct SerializedVector2Int
    {
        public int x;
        public int y;

        public SerializedVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2Int(SerializedVector2Int v) => new Vector2Int(v.x, v.y);
        public static implicit operator SerializedVector2Int(Vector2Int v) => new SerializedVector2Int(x: v.x, y: v.y);
    }
}
