using Celezt.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateExtension]
    public class ActorExtension : Extension<DialogueAsset>
    {
        [SerializeField]
        private int _integer = 1;
        [SerializeField]
        private string _editorActor;
        [SerializeField]
        private int[] _ints = new int[] { 1, 2, 3 };

        private MutString _runtimeActor;
    }
}
