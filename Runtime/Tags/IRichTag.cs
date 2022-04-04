using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRichTag : ITagTrait
{
    public void EnterTag(string parameter, int layer, RangeInt range) { }
    public void ExitTag(string parameter, int layer, RangeInt range) { }
    public void ProcessTag(string parameter, int layer, RangeInt range) { }
}
