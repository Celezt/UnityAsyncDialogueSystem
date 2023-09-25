using Celezt.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomTimelineEditor(typeof(DSPlayableAsset))]
    public class DSClipEditor : ClipEditor 
    {
        private static readonly Dictionary<Type, IExtensionClipEditor> _extensionClipEditors;

        private static (Type Type, IExtension Extension)[] _drawBuffer = new (Type, IExtension)[128];

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as IExtensionCollection;

            int count = 0;
            foreach (var extension in asset)
            {
                Type type = extension.GetType();

                if (_extensionClipEditors.TryGetValue(type, out var editor))
                    _drawBuffer[count++] = (type, extension);
            }

            // Sort and draw based on the order.
            foreach (var (type, extension) in _drawBuffer.Take(count).OrderBy(x => _extensionClipEditors[x.Type].Order))
                _extensionClipEditors[type].OnDrawBackground(clip, region, extension);
        }

        static DSClipEditor()
        {
            _extensionClipEditors = new();

            foreach (Type type in ReflectionUtility.GetTypesWithAttribute<CustomExtensionClipEditorAttribute>(AppDomain.CurrentDomain))
            {
                if (type.GetInterface(nameof(IExtensionClipEditor)) == null)
                    continue;

                var attribute = (CustomExtensionClipEditorAttribute)Attribute.GetCustomAttribute(type, typeof(CustomExtensionClipEditorAttribute));
        
                _extensionClipEditors[attribute.BindType] = (IExtensionClipEditor)Activator.CreateInstance(type);
            }
        }
    }
}
