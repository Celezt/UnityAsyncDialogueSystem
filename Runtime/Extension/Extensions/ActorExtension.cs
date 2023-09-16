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
        private string _editorActor;

        private MutString _runtimeActor;
    }
}
