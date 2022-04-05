using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    public void Initialize(Vector2 position);
    public void Draw();
}
