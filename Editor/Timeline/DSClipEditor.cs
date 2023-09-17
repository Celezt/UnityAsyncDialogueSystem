using Celezt.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem.Editor
{
    [CustomTimelineEditor(typeof(DialogueAsset))]
    public class DSClipEditor : ClipEditor
    {
        private static readonly Dictionary<Type, ExtensionDrawer> _extensionDrawers;
        private static readonly FieldInfo _typeInfo = typeof(CustomPropertyDrawer)
                                                        .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);

        private static (ExtensionDrawer Drawer, IExtension Extension)[] _drawBuffer = new (ExtensionDrawer, IExtension)[128];

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as IExtensionCollection;

            int count = 0;
            foreach (var extension in asset)
            {
                Type type = extension.GetType();

                if (_extensionDrawers.TryGetValue(type, out var drawer))
                    _drawBuffer[count++] = (drawer, extension);
            }

            // Sort and draw based on the order.
            foreach (var (drawer, extension) in _drawBuffer.Take(count).OrderBy(x => x.Drawer.Order))
                drawer.Internal_OnDrawBackground(clip, region, extension);
        }

        static DSClipEditor()
        {
            _extensionDrawers = new();

            foreach (Type type in ReflectionUtility.GetTypesWithAttribute<CustomPropertyDrawer>(AppDomain.CurrentDomain))
            {
                if (type == typeof(ExtensionDrawer) || !typeof(ExtensionDrawer).IsAssignableFrom(type))
                    continue;

                var attribute = (CustomPropertyDrawer)Attribute.GetCustomAttribute(type, typeof(CustomPropertyDrawer));
                Type objectType = (Type)_typeInfo.GetValue(attribute);

                _extensionDrawers[objectType] = (ExtensionDrawer)Activator.CreateInstance(type);
            }
        }
    }
}
