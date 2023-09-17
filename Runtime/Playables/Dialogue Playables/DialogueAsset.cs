using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using Celezt.Text;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

namespace Celezt.DialogueSystem
{
    public class DialogueAsset : DSPlayableAsset, IExtensionCollection
    {
        public double StartTime => Clip.start;
        public double EndTime => Clip.end;
        public float TimeDuration => (float)(EndTime - StartTime);
        public IReadOnlyList<IExtension> Extensions => _extensions;

#if UNITY_EDITOR
        internal bool HasUpdated { get; set; }
#endif

        [SerializeReference]
        private List<IExtension> _extensions = new();

        public void AddExtension(IExtensionCollection? extensions)
            => ExtensionUtility.AddExtensions(this, extensions, _extensions);

        public void AddExtension(IExtension? extension, UnityEngine.Object? reference = null)
            => ExtensionUtility.AddExtension(this, extension, _extensions, reference);

        public void RemoveExtension(Type type)
            => ExtensionUtility.RemoveExtension(type, _extensions);

        public void MoveUpExtension(Type type)
            => ExtensionUtility.MoveUpExtension(type, _extensions);
        public void MoveDownExtension(Type type)
            => ExtensionUtility.MoveDownExtension(type, _extensions);

        IEnumerator<IExtension> IEnumerable<IExtension>.GetEnumerator() => _extensions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _extensions.GetEnumerator();

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
