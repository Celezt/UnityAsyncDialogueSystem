using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITag : ITagTrait
{
    public void EnterTag(string parameter, int layer) { }
    public void ExitTag(string parameter, int layer) { }
    public void ProcessTag(string parameter, int layer) { }
}
