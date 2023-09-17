using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    // https://forum.unity.com/threads/locate-custompropertydrawer-from-serializedobject.462405/#post-3003847
    public class PropertyHandler
    {
        public PropertyDrawer propertyDrawer => _propertyDrawerInfo.GetValue(_handler, null) as PropertyDrawer;
        public Type Type => _type;

        private readonly static MethodInfo _getHandler = Type.GetType("UnityEditor.ScriptAttributeUtility, UnityEditor")
                                                    .GetMethod("GetHandler", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static object[] _getHandlerParams = new object[1];

        private object _handler;
        private Type _type;

        private PropertyInfo _propertyDrawerInfo;

        public PropertyHandler(SerializedProperty property)
        {
            _getHandlerParams[0] = property;
            _handler = _getHandler.Invoke(null, _getHandlerParams);
            _type = _handler.GetType();
            _propertyDrawerInfo = _type.GetProperty("propertyDrawer", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static PropertyHandler GetHandler(SerializedProperty property)
            => new PropertyHandler(property);
    }
}
