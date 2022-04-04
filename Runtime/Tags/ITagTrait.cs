using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITagTrait
{
    /// <summary>
    /// Called on awake.
    /// </summary>
    public void Awake() { }
    /// <summary>
    /// Called each time it is activated.
    /// </summary>
    public void OnActive() { }
}
