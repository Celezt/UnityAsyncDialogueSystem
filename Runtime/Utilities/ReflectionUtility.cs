using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public static class ReflectionUtility
    {
        private static readonly HashSet<string> _internalAssemblyNames = new()
        {
            "mscorlib",
            "System",
            "System.Core",
            "System.Security.Cryptography.Algorithms",
            "System.Net.Http",
            "System.Data",
            "System.Runtime.Serialization",
            "System.Xml.Linq",
            "System.Numerics",
            "System.Xml",
            "System.Configuration",
            "ExCSS.Unity",
            "Unity.Cecil",
            "Unity.CompilationPipeline.Common",
            "Unity.SerializationLogic",
            "Unity.TestTools.CodeCoverage.Editor",
            "Unity.ScriptableBuildPipeline.Editor",
            "Unity.Addressables.Editor",
            "Unity.ScriptableBuildPipeline",
            "Unity.CollabProxy.Editor",
            "Unity.Timeline.Editor",
            "Unity.PerformanceTesting.Tests.Runtime",
            "Unity.Settings.Editor",
            "Unity.PerformanceTesting",
            "Unity.PerformanceTesting.Editor",
            "Unity.Rider.Editor",
            "Unity.ResourceManager",
            "Unity.TestTools.CodeCoverage.Editor.OpenCover.Mono.Reflection",
            "Unity.PerformanceTesting.Tests.Editor",
            "Unity.TextMeshPro",
            "Unity.Timeline",
            "Unity.Addressables",
            "Unity.TestTools.CodeCoverage.Editor.OpenCover.Model",
            "Unity.VisualStudio.Editor",
            "Unity.TextMeshPro.Editor",
            "Unity.VSCode.Editor",
            "UnityEditor",
            "UnityEditor.UI",
            "UnityEditor.TestRunner",
            "UnityEditor.CacheServer",
            "UnityEditor.WindowsStandalone.Extensions",
            "UnityEditor.Graphs",
            "UnityEditor.UnityConnectModule",
            "UnityEditor.UIServiceModule",
            "UnityEditor.UIElementsSamplesModule",
            "UnityEditor.UIElementsModule",
            "UnityEditor.SceneTemplateModule",
            "UnityEditor.PackageManagerUIModule",
            "UnityEditor.GraphViewModule",
            "UnityEditor.CoreModule",
            "UnityEngine",
            "UnityEngine.UI",
            "UnityEngine.XRModule",
            "UnityEngine.WindModule",
            "UnityEngine.VirtualTexturingModule",
            "UnityEngine.TestRunner",
            "UnityEngine.VideoModule",
            "UnityEngine.VehiclesModule",
            "UnityEngine.VRModule",
            "UnityEngine.VFXModule",
            "UnityEngine.UnityWebRequestWWWModule",
            "UnityEngine.UnityWebRequestTextureModule",
            "UnityEngine.UnityWebRequestAudioModule",
            "UnityEngine.UnityWebRequestAssetBundleModule",
            "UnityEngine.UnityWebRequestModule",
            "UnityEngine.UnityTestProtocolModule",
            "UnityEngine.UnityCurlModule",
            "UnityEngine.UnityConnectModule",
            "UnityEngine.UnityAnalyticsModule",
            "UnityEngine.UmbraModule",
            "UnityEngine.UNETModule",
            "UnityEngine.UIElementsNativeModule",
            "UnityEngine.UIElementsModule",
            "UnityEngine.UIModule",
            "UnityEngine.TilemapModule",
            "UnityEngine.TextRenderingModule",
            "UnityEngine.TextCoreModule",
            "UnityEngine.TerrainPhysicsModule",
            "UnityEngine.TerrainModule",
            "UnityEngine.TLSModule",
            "UnityEngine.SubsystemsModule",
            "UnityEngine.SubstanceModule",
            "UnityEngine.StreamingModule",
            "UnityEngine.SpriteShapeModule",
            "UnityEngine.SpriteMaskModule",
            "UnityEngine.SharedInternalsModule",
            "UnityEngine.ScreenCaptureModule",
            "UnityEngine.RuntimeInitializeOnLoadManagerInitializerModule",
            "UnityEngine.ProfilerModule",
            "UnityEngine.Physics2DModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.PerformanceReportingModule",
            "UnityEngine.ParticleSystemModule",
            "UnityEngine.LocalizationModule",
            "UnityEngine.JSONSerializeModule",
            "UnityEngine.InputLegacyModule",
            "UnityEngine.InputModule",
            "UnityEngine.ImageConversionModule",
            "UnityEngine.IMGUIModule",
            "UnityEngine.HotReloadModule",
            "UnityEngine.GridModule",
            "UnityEngine.GameCenterModule",
            "UnityEngine.GIModule",
            "UnityEngine.DirectorModule",
            "UnityEngine.DSPGraphModule",
            "UnityEngine.CrashReportingModule",
            "UnityEngine.CoreModule",
            "UnityEngine.ClusterRendererModule",
            "UnityEngine.ClusterInputModule",
            "UnityEngine.ClothModule",
            "UnityEngine.AudioModule",
            "UnityEngine.AssetBundleModule",
            "UnityEngine.AnimationModule",
            "UnityEngine.AndroidJNIModule",
            "UnityEngine.AccessibilityModule",
            "UnityEngine.ARModule",
            "UnityEngine.AIModule",
            "SyntaxTree.VisualStudio.Unity.Bridge",
            "nunit.framework",
            "Newtonsoft.Json",
            "ReportGeneratorMerged",
            "Unrelated",
            "netstandard",
            "SyntaxTree.VisualStudio.Unity.Messaging"
        };

        public static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly) where T : Attribute =>
            assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(T)));

        public static IEnumerable<Type> GetTypesWithAttribute<T>(AppDomain appDomain) where T : Attribute
        {
            foreach (Assembly assembly in appDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                if (_internalAssemblyNames.Contains(assembly.GetName().Name))
                    continue;

                foreach (Type type in GetTypesWithAttribute<T>(assembly))
                    yield return type;
            }
        }

        private static IEnumerable<Type> GetDerivedTypes<T>(Assembly assembly) =>
            assembly.GetTypes().Where(t => t != typeof(T) && typeof(T).IsAssignableFrom(t));

        public static IEnumerable<Type> GetDerivedTypes<T>(AppDomain appDomain)
        {
            foreach (Assembly assembly in appDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                if (_internalAssemblyNames.Contains(assembly.GetName().Name))
                    continue;

                foreach (Type type in GetDerivedTypes<T>(assembly))
                    yield return type;
            }
        }

        public static void SetValue(this MemberInfo member, object property, object value)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(property, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(property, value, null);
                    break;
                default:
                    throw new ArgumentException("Property must be of type 'FieldInfo' or 'PropertyInfo'", nameof(property));
            }
        }

        public static object GetValue(this MemberInfo member, object property) => member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).GetValue(property),
            MemberTypes.Property => ((PropertyInfo)member).GetValue(property, null),
            _ => throw new ArgumentException("Property must be of type 'FieldInfo' or 'PropertyInfo'", nameof(property)),
        };

        public static Type GetUnderlyingType(this MemberInfo member) => member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).FieldType,
            MemberTypes.Property => ((PropertyInfo)member).PropertyType,
            MemberTypes.Event => ((EventInfo)member).EventHandlerType,
            _ => throw new ArgumentException("MemberInfo must be if type 'FieldInfo', 'PropertyInfo' or 'EventInfo'", nameof(member)),
        };
    }
}
